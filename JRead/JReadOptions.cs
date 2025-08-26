namespace JRead;

public class JReadOptions
{
    /// <summary>
    /// Custom history for the JRead instance
    /// </summary>
    public JReadHistory? CustomHistory { get; set; }
    public bool EnableDebug { get; set; } = false;
    public bool AddReturnedValueToHistory { get; set; } = true;
    /// <summary>
    /// If true, pressing Escape will return the original input instead of an null
    /// </summary>
    public bool EscapingReturnsTheOriginalInput { get; set; } = true;

    /// <summary>
    /// List of autocomplete suggestions
    /// </summary>
    public List<string> AutoCompleteItems { get; set; } = new List<string>();
    
    /// <summary>
    /// If true, shows autocomplete suggestions in grey text
    /// </summary>
    public bool EnableAutoComplete { get; set; } = true;
    
    /// <summary>
    /// Minimum characters before showing autocomplete suggestions
    /// </summary>
    public int AutoCompleteMinLength { get; set; } = 1;

    /// <summary>
    /// If true, autocomplete matching is case sensitive
    /// </summary>
    public bool AutoCompleteCaseSensitive { get; set; } = false;
    /// <summary>
    /// If true, masks input characters (e.g. for passwords)
    /// </summary>
    public bool EnableMaskedInput { get; set; } = false;
    public char MaskedInputChar { get; set; } = '*';

    internal CursorPos _cursorPos = new();
}