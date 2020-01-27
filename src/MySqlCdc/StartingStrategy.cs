namespace MySqlCdc
{
    internal enum StartingStrategy
    {
        FromStart,
        FromEnd,
        FromPosition,
        FromGtid
    }
}
