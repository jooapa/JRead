namespace JRead;

public class JReadOptions
{
    public JReadHistory? CustomHistory { get; set; }
    public bool EnableDebug { get; set; } = false;
    public bool AddReturnedValueToHistory { get; set; } = true;
    /// <summary>
    /// If true, pressing Escape will return the original input instead of an null
    /// </summary>
    public bool EscapingReturnsTheOriginalInput { get; set; } = true;

    internal CursorPos _cursorPos = new();
}