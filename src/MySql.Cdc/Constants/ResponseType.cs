namespace MySql.Cdc.Constants
{
    public enum ResponseType
    {
        Ok = 0x00,
        Error = 0xFF,
        AuthenticationSwitch = 0xFE,
    }
}
