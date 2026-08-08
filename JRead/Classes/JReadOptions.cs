namespace JRead;

public class JReadOptions
{
    /// <summary>
    /// Custom history for the JRead instance
    /// </summary>
    public JReadHistory? CustomHistory { get; set; }
    /// <summary>
    /// will print debug information to the console when key is pressed
    /// </summary>
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
    /// <summary>
    /// If true, adds a new line to console. not the input but the cursor
    /// </summary>
    public bool NewLineOnExit { get; set; } = true;

    /// <summary>
    /// Maximum number of characters to display in the input area (windowed view).
    /// If set, the input area will never show more than this many characters at once.
    /// The input can be longer, but only a window of MaxDisplayLength characters is visible and editable.
    /// </summary>
    public int? MaxDisplayLength { get; set; } = null;

    /// <summary>
    /// If true, MaxDisplayLength is subtracted from the available console width, reserving space at the end of the line.
    /// </summary>
    public bool SubtractFromAvailableSpace { get; set; } = false;

    /// <summary>
    /// Callback invoked when the up arrow is pressed. Return a new string to replace the input,
    /// or null to fall through to the default history navigation.
    /// </summary>
    public Func<string, string?>? OnUpArrow { get; set; }

    /// <summary>
    /// Callback invoked when the down arrow is pressed. Return a new string to replace the input,
    /// or null to fall through to the default history navigation.
    /// </summary>
    public Func<string, string?>? OnDownArrow { get; set; }

    internal CursorPos _cursorPos = new();
}