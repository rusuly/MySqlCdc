namespace MySqlCdc.Commands;

/// <summary>
/// Command types are included in body headers to identify the commands
/// </summary>
internal enum CommandType : byte
{
    Sleep = 0,
    Quit = 1,
    InitDb = 2,
    Query = 3,
    FieldList = 4,
    CreateDb = 5,
    DropDb = 6,
    Refresh = 7,
    Shutdown = 8,
    Statistics = 9,
    ProcessInfo = 10,
    Connect = 11,
    ProcessKill = 12,
    Debug = 13,
    Ping = 14,
    Time = 15,
    DelayedInsert = 16,
    ChangeUser = 17,
    BinlogDump = 18,

    RegisterSlave = 21,
    BinlogDumpGtid = 30
}