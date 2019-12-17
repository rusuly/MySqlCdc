namespace MySql.Cdc.Constants
{
    public enum ResponseType
    {
        Ok = 0x00,
        Error = 0xFF,
        EndOfFile = 0xFE,
        AuthenticationSwitch = 0xFE,
    }
}
