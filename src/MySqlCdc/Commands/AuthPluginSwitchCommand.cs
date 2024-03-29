using MySqlCdc.Protocol;

namespace MySqlCdc.Commands;

internal class AuthPluginSwitchCommand : ICommand
{
    public string Password { get; }
    public string Scramble { get; }
    public string AuthPluginName { get; }

    public AuthPluginSwitchCommand(string password, string scramble, string authPluginName)
    {
        Password = password;
        Scramble = scramble;
        AuthPluginName = authPluginName;
    }

    public byte[] Serialize()
    {
        var writer = new PacketWriter();
        writer.WriteByteArray(Extensions.GetEncryptedPassword(Password, Scramble, AuthPluginName));
        return writer.CreatePacket();
    }
}