namespace MySqlCdc.Constants;

internal enum ResponseType
{
    Ok = 0,
    Error = 255,
    EndOfFile = 254,
    AuthPluginSwitch = 254,
}