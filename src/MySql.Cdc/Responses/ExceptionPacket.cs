using System;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Packets
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
