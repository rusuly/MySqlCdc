using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MySqlCdc.Constants;
using MySqlCdc.Packets;
using System.Security.Cryptography;
using System.Text;

namespace MySqlCdc;

internal static class Extensions
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
    
    public static void ThrowIfErrorPacket(byte[] packet, string message)
    {
        if (packet[0] == (byte)ResponseType.Error)
        {
            var error = new ErrorPacket(packet[1..]);
            throw new InvalidOperationException($"{message} {error}");
        }
    }
    
    public static byte[] GetEncryptedPassword(string password, string scramble, string authPluginName)
    {
        HashAlgorithm sha = authPluginName switch
        {
            AuthPluginNames.MySqlNativePassword => SHA1.Create(),
            AuthPluginNames.CachingSha2Password => SHA256.Create(),
            _ => throw new NotSupportedException()
        };

        var passwordHash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        var concatHash = Encoding.UTF8.GetBytes(scramble).Concat(sha.ComputeHash(passwordHash)).ToArray();
        return Xor(passwordHash, sha.ComputeHash(concatHash));
    }

    public static byte[] Xor(byte[] array1, byte[] array2)
    {
        byte[] result = new byte[array1.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = (byte)(array1[i] ^ (array2[i % array2.Length]));
        return result;
    }
}