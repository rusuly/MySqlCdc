using MySqlCdc.Protocol;

namespace MySqlCdc.Commands
{
    public class AuthPluginSwitchCommand : ICommand
    {
        public string Password { get; private set; }
        public string Scramble { get; private set; }
        public string AuthPluginName { get; private set; }

        public AuthPluginSwitchCommand(string password, string scramble, string authPluginName)
        {
            Password = password;
            Scramble = scramble;
            AuthPluginName = authPluginName;
        }

        public byte[] CreatePacket(byte sequenceNumber)
        {
            var writer = new PacketWriter(sequenceNumber);
            writer.WriteByteArray(AuthenticateCommand.GetEncryptedPassword(Password, Scramble, AuthPluginName));
            return writer.CreatePacket();
        }
    }
}
