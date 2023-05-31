namespace MySqlCdc.Events;

/// <summary>
/// A USER_VAR_EVENT is written every time a statement uses a user defined variable.
/// <a href="https://mariadb.com/kb/en/user_var_event/">See more</a>
/// </summary>
public class UserVarEvent : BinlogEvent
{
    /// <summary>
    /// User variable name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// User variable value
    /// </summary>
    public VariableValue? Value { get; }

    /// <summary>
    /// Creates a new <see cref="UserVarEvent"/>.
    /// </summary>
    public UserVarEvent(EventHeader header, string name, VariableValue? value) : base(header)
    {
        Name = name;
        Value = value;
    }
}

/// <summary>
/// User variable value
/// </summary>
public class VariableValue
{
    /// <summary>
    /// Variable type
    /// </summary>
    public byte VariableType { get; }

    /// <summary>
    /// Collation number
    /// </summary>
    public int CollationNumber { get; }

    /// <summary>
    /// User variable value
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// flags
    /// </summary>
    public byte Flags { get; }

    /// <summary>
    /// Creates a new <see cref="VariableValue"/>.
    /// </summary>
    public VariableValue(byte variableType, int collationNumber, string value, byte flags)
    {
        VariableType = variableType;
        CollationNumber = collationNumber;
        Value = value;
        Flags = flags;
    }
}