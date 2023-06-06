using System.Runtime.CompilerServices;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using MySqlCdc.Network;
using MySqlCdc.Packets;

namespace MySqlCdc;

/// <summary>
/// MySql replication client streaming binlog events in real-time.
/// </summary>
public class BinlogClient
{
    private readonly ReplicaOptions _options = new();

    private IGtid? _gtid;
    private bool _transaction;

    /// <summary>
    /// Creates a new <see cref="BinlogClient"/>.
    /// </summary>
    /// <param name="configureOptions">The configure callback</param>
    public BinlogClient(Action<ReplicaOptions> configureOptions)
    {
        configureOptions(_options);
        
        if (_options.SslMode == SslMode.RequireVerifyCa || _options.SslMode == SslMode.RequireVerifyFull)
            throw new NotSupportedException($"{nameof(SslMode.RequireVerifyCa)} and {nameof(SslMode.RequireVerifyFull)} ssl modes are not supported");
    }

    /// <summary>
    /// Gets current replication state.
    /// </summary>
    public BinlogOptions State => _options.Binlog;

    /// <summary>
    /// Replicates binlog events from the server
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task completed when last event is read in non-blocking mode</returns>
    public async IAsyncEnumerable<IBinlogEvent> Replicate(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var (connection, databaseProvider) = await new Connector(_options).ConnectAsync(cancellationToken);

        // Clear on reconnect
        _gtid = null;
        _transaction = false;

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
            if (packet is IBinlogEvent binlogEvent)
            {
                // We stop replication if client code throws an exception
                // As a derived database may end up in an inconsistent state.
                yield return binlogEvent;

                // Commit replication state if there is no exception.
                UpdateGtidPosition(binlogEvent);
                UpdateBinlogPosition(binlogEvent);
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

    private void UpdateBinlogPosition(IBinlogEvent binlogEvent)
    {
        // Rows event depends on preceding TableMapEvent & we change the position
        // after we read them atomically to prevent missing mapping on reconnect.
        // Figure out something better as TableMapEvent can be followed by several row events.
        if (binlogEvent is TableMapEvent tableMapEvent)
            return;

        if (binlogEvent is RotateEvent rotateEvent)
        {
            _options.Binlog.Filename = rotateEvent.BinlogFilename;
            _options.Binlog.Position = rotateEvent.BinlogPosition;
        }
        else if (binlogEvent.Header.NextEventPosition > 0)
        {
            _options.Binlog.Position = binlogEvent.Header.NextEventPosition;
        }
    }
}
