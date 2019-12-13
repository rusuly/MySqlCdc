using System;
using System.Threading.Tasks;

namespace MySql.Cdc.Network
{
    public class PacketAwaiter : IDisposable
    {
        private static PacketAwaiter _instance = new PacketAwaiter();

        public TaskCompletionSource<byte[]> Caller { get; private set; }
        public TaskCompletionSource<int> Pusher { get; private set; }

        private PacketAwaiter()
        {
            Caller = new TaskCompletionSource<byte[]>();
            Pusher = new TaskCompletionSource<int>();
        }

        public void Reset()
        {
            Caller = new TaskCompletionSource<byte[]>();
            Pusher = new TaskCompletionSource<int>();
        }

        public static PacketAwaiter GetAwaiter()
        {
            return _instance;
        }

        public static void ResetAwaiter()
        {
            _instance = new PacketAwaiter();
        }

        public void Dispose() { }
    }
}
