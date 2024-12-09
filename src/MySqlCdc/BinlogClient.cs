using System.Runtime.CompilerServices;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using MySqlCdc.Network;
using MySqlCdc.Packets;

namespace MySqlCdc;

internal enum RowBasedTxState
{
    NotInTx,
    HandlingTableMaps,
    HandlingRowsEvents
}

/// <summary>
/// MySql replication client streaming binlog events in real-time.
/// </summary>
public class BinlogClient
{
    private readonly ReplicaOptions _options = new();

    private IGtid? _gtid;
    private bool _transaction;
    private RowBasedTxState _txState;
    private (string, long)? _reconnectPosition;

    /// <summary>
    /// Creates a new <see cref="BinlogClient"/>.
    /// </summary>
    /// <param name="configureOptions">The configure callback</param>
    public BinlogClient(Action<ReplicaOptions> configureOptions)
    {
        configureOptions(_options);

        if (_options.SslMode == SslMode.RequireVerifyCa || _options.SslMode == SslMode.RequireVerifyFull)
            throw new NotSupportedException(
                $"{nameof(SslMode.RequireVerifyCa)} and {nameof(SslMode.RequireVerifyFull)} ssl modes are not supported");
    }

    /// <summary>
    /// Gets current replication state.
    /// </summary>
    public BinlogOptions State => _options.Binlog;

    /// <summary>
    /// Replicates binlog events from the server.
    /// <br/>
    /// If an exception is thrown, it is safe to call this method again on the same binlog client to resume replication,
    /// with one exception: if row-based logging is used and the exception occurs while processing a transaction (a
    /// sequence of table-map and rows events before the transaction finish "xid" event), the replication will resume at
    /// the first table-map event, meaning some events will be streamed twice.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completed when last event is read in non-blocking mode. This can also happen under certain
    /// conditions in blocking mode, so prepare to resume replication if the stream ends.</returns>
    public async IAsyncEnumerable<(EventHeader, IBinlogEvent)> Replicate(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_reconnectPosition != null)
        {
            _options.Binlog.Filename = _reconnectPosition.Value.Item1;
            _options.Binlog.Position = _reconnectPosition.Value.Item2;
            _reconnectPosition = null;
        }

        var (connection, databaseProvider) = await new Connector(_options).ConnectAsync(cancellationToken);

        // Clear on reconnect
        _gtid = null;
        _transaction = false;
        _txState = RowBasedTxState.NotInTx;

        var configurator = new Configurator(_options, connection, databaseProvider);
        await configurator.AdjustStartingPosition(cancellationToken);
        await configurator.SetMasterHeartbeat(cancellationToken);
        await configurator.SetMasterBinlogChecksum(cancellationToken);
        await databaseProvider.DumpBinlogAsync(connection, _options, cancellationToken);

        var eventStreamReader = new EventStreamReader(databaseProvider.Deserializer);
        var channel = new EventStreamChannel(eventStreamReader, connection.Stream);
        var timeout = _options.HeartbeatInterval.Add(TimeSpan.FromMilliseconds(TimeoutConstants.Delta));

        await foreach (var packet in channel.ReadPacketAsync(timeout, cancellationToken)
                           .WithCancellation(cancellationToken))
        {
            if (packet is HeaderWithEvent binlogEvent)
            {
                // We stop replication if client code throws an exception
                // As a derived database may end up in an inconsistent state.
                yield return (binlogEvent.Header, binlogEvent.Event);

                // Commit replication state if there is no exception.
                UpdateGtidPosition(binlogEvent.Event);
                UpdateBinlogPosition(binlogEvent.Header, binlogEvent.Event);
            }
            else if (packet is EndOfFilePacket && !_options.Blocking)
                yield break;
            else if (packet is ErrorPacket error)
                throw new InvalidOperationException($"Event stream error. {error.ToString()}");
            else
                throw new InvalidOperationException($"Event stream unexpected error.");
        }
    }

    private void UpdateGtidPosition(IBinlogEvent binlogEvent)
    {
        if (_options.Binlog.StartingStrategy != StartingStrategy.FromGtid)
            return;

        if (binlogEvent is GtidEvent gtidEvent)
        {
            _gtid = gtidEvent.Gtid;
        }
        else if (binlogEvent is XidEvent xidEvent)
        {
            CommitGtid();
        }
        else if (binlogEvent is QueryEvent queryEvent)
        {
            if (string.IsNullOrWhiteSpace(queryEvent.SqlStatement))
                return;

            if (queryEvent.SqlStatement == "BEGIN")
            {
                _transaction = true;
            }
            else if (queryEvent.SqlStatement == "COMMIT" || queryEvent.SqlStatement == "ROLLBACK")
            {
                CommitGtid();
            }
            else if (!_transaction)
            {
                // Auto-commit query like DDL
                CommitGtid();
            }
        }
    }

    private void CommitGtid()
    {
        _transaction = false;
        if (_gtid != null)
            _options.Binlog.GtidState!.AddGtid(_gtid);
    }

    private void UpdateBinlogPosition(EventHeader header, IBinlogEvent binlogEvent)
    {
        // Rows event depends on preceding TableMapEvent & we change the position
        // after we read them atomically to prevent missing mapping on reconnect.
        switch (_txState, binlogEvent)
        {
            // We only allow reconnecting at the first of a set of table map events
            case (RowBasedTxState.NotInTx, TableMapEvent):
            case (RowBasedTxState.HandlingRowsEvents, TableMapEvent):
                _txState = RowBasedTxState.HandlingTableMaps;
                _reconnectPosition = (_options.Binlog.Filename, _options.Binlog.Position);
                break;
            case (RowBasedTxState.NotInTx, IBinlogRowsEvent):
                throw new InvalidOperationException("Unexpected row event without TableMapEvent.");
            // More table map events might come after a set of rows events
            case (RowBasedTxState.HandlingTableMaps, IBinlogRowsEvent):
                _txState = RowBasedTxState.HandlingRowsEvents;
                break;
            // Multiple table map or rows events can come in a row, and all of them except the first table map event are
            // invalid positions to reconnect
            case (RowBasedTxState.HandlingTableMaps, TableMapEvent):
            case (RowBasedTxState.HandlingRowsEvents, IBinlogRowsEvent):
                break;
            default:
                _txState = RowBasedTxState.NotInTx;
                _reconnectPosition = null;
                break;
        }

        if (binlogEvent is RotateEvent rotateEvent)
        {
            _options.Binlog.Filename = rotateEvent.BinlogFilename;
            _options.Binlog.Position = rotateEvent.BinlogPosition;
        }
        else if (header.NextEventPosition > 0)
        {
            _options.Binlog.Position = header.NextEventPosition;
        }
    }
}