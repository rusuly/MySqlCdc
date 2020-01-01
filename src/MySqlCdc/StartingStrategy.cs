namespace MySqlCdc
{
    public enum StartingStrategy
    {
        FromStart,
        FromEnd,
        FromPosition,
        FromGtid
    }
}
