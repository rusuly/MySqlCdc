using System;
using System.Linq;
using System.Text;

namespace MySqlCdc.Providers.MySql
{
    /// <summary>
    /// Represents Uuid with little-endian bytes order unlike big-endian Guid.
    /// </summary>
    public class Uuid
    {
        private readonly byte[] _data;
        private readonly string _uuid;

        /// <summary>
        /// Creates a new <see cref="Uuid"/>.
        /// </summary>
        public Uuid(byte[] data)
        {
            if (data.Length != 16)
                throw new ArgumentException("Uuid requires byte[16]");

            var sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                if (i == 4 || i == 6 || i == 8 || i == 10)
                    sb.Append("-");

                sb.AppendFormat("{0:x2}", data[i]);
            }

            _data = data;
            _uuid = sb.ToString();
        }

        /// <summary>
        /// Parses <see cref="Uuid"/> from string representation.
        /// </summary>
        public static Uuid Parse(string value)
        {
            var hex = value.Replace("-", "");
            var data = Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
            return new Uuid(data);
        }

        /// <summary>
        /// Returns byte[] representation of the Uuid.
        /// </summary>
        public byte[] ToByteArray() => _data;

        /// <summary>
        /// Compares two values for equality
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            if (obj is Uuid other)
                return _data.SequenceEqual(other.ToByteArray());

            return false;
        }

        /// <summary>
        /// Compares two values for equality
        /// </summary>
        public static bool operator ==(Uuid uuid1, Uuid uuid2)
        {
            if (object.ReferenceEquals(uuid1, uuid2))
                return true;

            return uuid1.Equals(uuid2);
        }

        /// <summary>
        /// Compares two values for equality
        /// </summary>
        public static bool operator !=(Uuid uuid1, Uuid uuid2)
        {
            return !(uuid1 == uuid2);
        }

        /// <summary>
        /// Calls to this method always throw a <see cref="NotSupportedException"/>.
        /// </summary>
        public override int GetHashCode() => _uuid.GetHashCode();

        /// <summary>
        /// Returns string representation of the GtidSet.
        /// </summary>
        public override string ToString() => _uuid;
    }
}