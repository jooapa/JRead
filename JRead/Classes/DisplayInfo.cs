namespace JRead;

internal struct DisplayInfo
{
    public string BeforeCursor { get; set; }
    public string Suggestion { get; set; }
    public string AfterCursor { get; set; }
    public int CursorPosition { get; set; }
    public bool HasSuggestion { get; set; }
}