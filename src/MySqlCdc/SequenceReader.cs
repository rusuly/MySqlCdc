#if NETSTANDARD2_0
using System;
using System.Buffers;
using System.IO;

namespace MySqlCdc
{
    /// <summary>
    /// Used in netstandard2.0 to emulate netstandard2.1 SequenceReader.
    /// The class is rewritten BufferReader from https://github.com/StackExchange/StackExchange.Redis
    /// The MIT license can be found here https://github.com/StackExchange/StackExchange.Redis/blob/master/LICENSE
    /// </summary>
    internal ref struct SequenceReader<T>
    {
        public ReadOnlySequence<byte> Sequence { get; }
        private SequencePosition _lastSnapshotPosition;
        private long _lastSnapshotBytes;
        private ReadOnlySequence<byte>.Enumerator _iterator;
        private ReadOnlySpan<byte> _current;

        public int OffsetThisSpan { get; private set; }
        public int Consumed { get; private set; }
        public int RemainingThisSpan { get; private set; }
        public int Remaining => RemainingThisSpan;
        public bool IsEmpty => RemainingThisSpan == 0;

        public SequenceReader(ReadOnlySequence<byte> buffer)
        {
            Sequence = buffer;
            _lastSnapshotPosition = buffer.Start;
            _lastSnapshotBytes = 0;
            _iterator = buffer.GetEnumerator();
            _current = default;
            OffsetThisSpan = RemainingThisSpan = Consumed = 0;

            FetchNextSegment();
        }

        private bool FetchNextSegment()
        {
            do
            {
                if (!_iterator.MoveNext())
                {
                    OffsetThisSpan = RemainingThisSpan = 0;
                    return false;
                }

                _current = _iterator.Current.Span;
                OffsetThisSpan = 0;
                RemainingThisSpan = _current.Length;
            } while (IsEmpty); // skip empty segments, they don't help us!

            return true;
        }

        public void Advance(int count)
        {
            if (!TryAdvance(count)) throw new EndOfStreamException();
        }

        public bool TryAdvance(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            do
            {
                var available = RemainingThisSpan;
                if (count <= available)
                {
                    // consume part of this span
                    Consumed += count;
                    RemainingThisSpan -= count;
                    OffsetThisSpan += count;

                    if (count == available) FetchNextSegment(); // burned all of it; fetch next
                    return true;
                }

                // consume all of this span
                Consumed += available;
                count -= available;
            } while (FetchNextSegment());
            return false;
        }


        // makes an internal note of where we are, as a SequencePosition; useful
        // to avoid having to use buffer.Slice on huge ranges
        private SequencePosition SnapshotPosition()
        {
            var consumed = Consumed;
            var delta = consumed - _lastSnapshotBytes;
            if (delta == 0) return _lastSnapshotPosition;

            var pos = Sequence.GetPosition(delta, _lastSnapshotPosition);
            _lastSnapshotBytes = consumed;
            return _lastSnapshotPosition = pos;
        }

        public bool TryRead(out byte value)
        {
            if (IsEmpty)
            {
                value = 0;
                return false;
            }

            value = _current[OffsetThisSpan];
            Advance(1);
            return true;
        }

        public void TryReadTo(out ReadOnlySequence<byte> sequence, byte terminator)
        {
            var from = SnapshotPosition();
            var buffer = Sequence.Slice(from);

            var position = buffer.PositionOf(terminator);

            sequence = buffer.Slice(0, position.Value);
            Advance((int)sequence.Length + 1);
        }
    }
}
#endif