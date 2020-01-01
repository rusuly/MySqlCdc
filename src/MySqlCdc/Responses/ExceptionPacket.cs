using System;
using MySqlCdc.Protocol;

namespace MySqlCdc.Packets
{
    /// <summary>
    /// Passes an exception from packet channel thread to application code.
    /// </summary>
    public class ExceptionPacket : IPacket
    {
        public Exception Exception { get; }

        public ExceptionPacket(Exception exception)
        {
            Exception = exception;
        }
    }
}
