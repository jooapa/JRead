namespace JRead;

internal struct InputState
{
    public string Input { get; set; }
    public int CursorPosition { get; set; }

    public InputState(string input, int cursorPosition)
    {
        Input = input;
        CursorPosition = cursorPosition;
    }
}