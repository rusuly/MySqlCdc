using System;
using System.Threading;
using System.Threading.Tasks;

namespace MySql.Cdc
{
    public static class TimeoutExtensions
    {
        public static async Task<TResult> WithTimeout<TResult>(this Task<TResult> task, TimeSpan timeoutSpan, string timeoutMessage)
        {
            var timeout = (int)timeoutSpan.TotalMilliseconds;
            var cts = new CancellationTokenSource();

            if (task == await Task.WhenAny(task, Task.Delay(timeout, cts.Token)).ConfigureAwait(false))
            {
                cts.Cancel();
                return await task.ConfigureAwait(false);
            }
            throw new TimeoutException(timeoutMessage);
        }
    }
}
