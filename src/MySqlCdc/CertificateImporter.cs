#if NETSTANDARD2_0
using System;
using System.Buffers;
using System.Linq;
using System.Security.Cryptography;
using MySqlCdc.Protocol;

namespace MySqlCdc
{
    /// <summary>
    /// Used in netstandard2.0 to parse MySQL certificate.
    /// Info is taken from https://stackoverflow.com/a/32243171
    /// </summary>
    public static class CertificateImporter
    {
        /// <summary>
        /// Encoded OID sequence for PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
        /// </summary>
        private static readonly byte[] OidSequence =
            { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };

        public static RSAParameters DecodePublicKey(byte[] x509Key)
        {
            var reader = new PacketReader(new ReadOnlySequence<byte>(x509Key));

            var twobytes = reader.ReadInt(2);
            var skipBytes = twobytes switch
            {
                0x8130 => 1,
                0x8230 => 2,
                _ => throw new FormatException()
            };
            reader.Skip(skipBytes);

            var sequence = reader.ReadByteArraySlow(15);
            if (!sequence.SequenceEqual(OidSequence))
                throw new FormatException("Sequence is not OID");

            twobytes = reader.ReadInt(2);
            skipBytes = twobytes switch
            {
                0x8103 => 1,
                0x8203 => 2,
                _ => throw new FormatException()
            };
            reader.Skip(skipBytes);

            if (reader.ReadInt(1) != 0x00)
                throw new FormatException(); // Null byte

            twobytes = reader.ReadInt(2);
            skipBytes = twobytes switch
            {
                0x8130 => 1,
                0x8230 => 2,
                _ => throw new FormatException()
            };
            reader.Skip(skipBytes);

            // Read modulus size
            twobytes = reader.ReadInt(2);
            int modulusSize = twobytes switch
            {
                0x8102 => reader.ReadBigEndianInt(1),
                0x8202 => reader.ReadBigEndianInt(2),
                _ => throw new FormatException()
            };

            byte[] modulus = reader.ReadByteArraySlow(modulusSize);
            if (modulus[0] == 0x00)
                modulus = modulus.Skip(1).ToArray();

            if (reader.ReadInt(1) != 0x02)
                throw new FormatException();

            var exponentSize = reader.ReadInt(1);
            byte[] exponent = reader.ReadByteArraySlow(exponentSize);
            return new RSAParameters { Modulus = modulus, Exponent = exponent };
        }
    }
}
#endif