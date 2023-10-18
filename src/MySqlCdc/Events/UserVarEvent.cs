namespace MySqlCdc.Events;

/// <summary>
/// A USER_VAR_EVENT is written every time a statement uses a user defined variable.
/// <a href="https://mariadb.com/kb/en/user_var_event/">See more</a>
/// </summary>
/// <remarks>
/// Creates a new <see cref="UserVarEvent"/>.
/// </remarks>
public record UserVarEvent(string Name, VariableValue? Value) : IBinlogEvent
{
    /// <summary>
    /// User variable name
    /// </summary>
    public string Name { get; } = Name;

    /// <summary>
    /// User variable value
    /// </summary>
    public VariableValue? Value { get; } = Value;
}

/// <summary>
/// User variable value
/// </summary>
/// <remarks>
/// Creates a new <see cref="VariableValue"/>.
/// </remarks>
public record VariableValue(byte VariableType, int CollationNumber, string Value, byte Flags)
{
    /// <summary>
    /// Variable type
    /// </summary>
    public byte VariableType { get; } = VariableType;

    /// <summary>
    /// Collation number
    /// </summary>
    public int CollationNumber { get; } = CollationNumber;

    /// <summary>
    /// User variable value
    /// </summary>
    public string Value { get; } = Value;

    /// <summary>
    /// flags
    /// </summary>
    public byte Flags { get; } = Flags;
}