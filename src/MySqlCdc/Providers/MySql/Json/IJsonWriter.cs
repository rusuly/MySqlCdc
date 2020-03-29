using System;

namespace MySqlCdc.Providers.MySql
{
    /// <summary>
    /// Provides API for generating JSON in forward-only way.
    /// </summary>
    public interface IJsonWriter
    {
        /// <summary>
        /// Writes a key in an object.
        /// </summary>
        void WriteKey(string name);

        /// <summary>
        /// Writes the the beginning of an object.
        /// </summary>
        void WriteStartObject();

        /// <summary>
        /// Writes the the beginning of an array.
        /// </summary>
        void WriteStartArray();

        /// <summary>
        /// Writes the the end of an object.
        /// </summary>
        void WriteEndObject();

        /// <summary>
        /// Writes the the end of an array.
        /// </summary>
        void WriteEndArray();

        /// <summary>
        /// Writes Int16 value.
        /// </summary>
        void WriteValue(Int16 value);

        /// <summary>
        /// Writes UInt16 value.
        /// </summary>
        void WriteValue(UInt16 value);

        /// <summary>
        /// Writes Int32 value.
        /// </summary>
        void WriteValue(Int32 value);

        /// <summary>
        /// Writes UInt32 value.
        /// </summary>
        void WriteValue(UInt32 value);

        /// <summary>
        /// Writes Int64 value.
        /// </summary>
        void WriteValue(Int64 value);

        /// <summary>
        /// Writes UInt64 value.
        /// </summary>
        void WriteValue(UInt64 value);

        /// <summary>
        /// Writes double value.
        /// </summary>
        void WriteValue(double value);

        /// <summary>
        /// Writes string value.
        /// </summary>
        void WriteValue(string value);

        /// <summary>
        /// Writes bool value.
        /// </summary>
        void WriteValue(bool value);

        /// <summary>
        /// Writes null literal.
        /// </summary>
        void WriteNull();
    }
}
