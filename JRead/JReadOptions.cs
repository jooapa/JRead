namespace JRead;

public class JReadOptions
{
    public JReadHistory? CustomHistory { get; set; }
    public bool EnableDebug { get; set; } = false;
    public bool AddReturnedValueToHistory { get; set; } = true;
    internal CursorPos _cursorPos = new();
}