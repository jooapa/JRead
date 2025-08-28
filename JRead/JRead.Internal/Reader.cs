namespace JRead.Internal;

internal static class Reader
{
    internal static readonly char[] AutocompleteWordBoundaries = { ' ', '"', '\'', '/', '(', ')', '[', ']', '{', '}', ',', '.', ';', ':', '!', '?', '@', '#', '$', '%', '^', '&', '*', '+', '=', '|', '\\', '<', '>', '~', '`' };

    // maybe i should not change options globally with changes
    internal static bool _enableAutoComplete = false;

    internal static string? ReadInternal(string? prefillText, JReadOptions? options = null, string beginningText = "")
    {

#pragma warning disable CS8321
        bool IsCtrlKeyPressed(ConsoleKeyInfo keyInfo) => (keyInfo.Modifiers & ConsoleModifiers.Control) != 0;
        bool IsShiftKeyPressed(ConsoleKeyInfo keyInfo) => (keyInfo.Modifiers & ConsoleModifiers.Shift) != 0;
        bool IsAltKeyPressed(ConsoleKeyInfo keyInfo) => (keyInfo.Modifiers & ConsoleModifiers.Alt) != 0;
#pragma warning restore CS8321
        if (!string.IsNullOrEmpty(beginningText))
            Console.Write(beginningText);

        string input = prefillText ?? "";
        int cursorPosition = input.Length;
        int historyIndex = -1; // -1 means current input, 0+ means history item
        string originalInput = input;
        ConsoleKeyInfo key;
        options ??= new JReadOptions();

        {
            // Set _enableAutoComplete to options value at first run
            _enableAutoComplete = options.EnableAutoComplete;

            // dont show autocomplete suggestions if masked input is enabled
            if (options.EnableMaskedInput)
            {
                _enableAutoComplete = false;
            }
        }

        // Undo/Redo functionality
        var undoStack = new Stack<InputState>();
        var redoStack = new Stack<InputState>();

        // Helper method to save current state for undo
        void SaveStateForUndo()
        {
            undoStack.Push(new InputState(input, cursorPosition));
            redoStack.Clear(); // Clear redo stack when new action is performed

            // Limit undo stack size to prevent memory issues
            if (undoStack.Count > 100)
            {
                var tempStack = new Stack<InputState>();
                for (int i = 0; i < 50; i++)
                {
                    if (undoStack.Count > 0)
                        tempStack.Push(undoStack.Pop());
                }
                undoStack.Clear();
                while (tempStack.Count > 0)
                {
                    undoStack.Push(tempStack.Pop());
                }
            }
        }

        // Save initial state
        SaveStateForUndo();

        options._cursorPos = new CursorPos
        {
            Left = Console.CursorLeft,
            Top = Console.CursorTop
        };

        // get if global or local history
        JReadHistory history = options.CustomHistory ?? JRead.History;

        // Track terminal size for resize detection
        int lastWindowWidth = Console.WindowWidth;
        int lastWindowHeight = Console.WindowHeight;

        // Write initial input
        DrawLine(input, cursorPosition, options);

        do
        {
            // Check for terminal resize before reading key
            if (Console.WindowWidth != lastWindowWidth || Console.WindowHeight != lastWindowHeight)
            {
                lastWindowWidth = Console.WindowWidth;
                lastWindowHeight = Console.WindowHeight;

                // Redraw the line after resize
                DrawLine(input, cursorPosition, options);
            }

            key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    if (options.EscapingReturnsTheOriginalInput)
                    {
                        return originalInput;
                    }

                    if (options.NewLineOnExit)
                        Console.WriteLine();

                    return null;

                case ConsoleKey.Enter:
                    // Add to history if enabled
                    if (options.AddReturnedValueToHistory)
                    {
                        history.Add(input);
                    }

                    if (options.NewLineOnExit)
                        Console.WriteLine();

                    return input;

                case ConsoleKey.Backspace:
                    if (input.Length > 0 && cursorPosition > 0)
                    {
                        // Save state for undo before modification
                        SaveStateForUndo();

                        // Reset history navigation when editing
                        historyIndex = -1;

                        input = input.Remove(cursorPosition - 1, 1);
                        cursorPosition--;
                        DrawLine(input, cursorPosition, options);
                    }
                    break;
                case ConsoleKey.Delete:
                    if (input.Length > 0 && cursorPosition < input.Length)
                    {
                        // Save state for undo before modification
                        SaveStateForUndo();

                        // Reset history navigation when editing
                        historyIndex = -1;

                        input = input.Remove(cursorPosition, 1);
                        DrawLine(input, cursorPosition, options);
                    }
                    break;
                case ConsoleKey.UpArrow:
                    if (history.Count > 0)
                    {
                        if (historyIndex == -1)
                        {
                            // First time going into history, save current input and state for undo
                            SaveStateForUndo();
                            originalInput = input;
                            historyIndex = history.Count - 1;
                        }
                        else if (historyIndex > 0)
                        {
                            // Save current state before changing to different history item
                            SaveStateForUndo();
                            historyIndex--;
                        }

                        var historyItem = history.Get(historyIndex);
                        if (historyItem != null)
                        {
                            input = historyItem;
                            cursorPosition = input.Length;
                            DrawLine(input, cursorPosition, options);
                        }
                    }
                    break;

                case ConsoleKey.DownArrow:
                    if (history.Count > 0 && historyIndex != -1)
                    {
                        // Save current state before changing
                        SaveStateForUndo();

                        if (historyIndex < history.Count - 1)
                        {
                            historyIndex++;
                            var historyItem = history.Get(historyIndex);
                            if (historyItem != null)
                            {
                                input = historyItem;
                                cursorPosition = input.Length;
                                DrawLine(input, cursorPosition, options);
                            }
                        }
                        else
                        {
                            // Go back to original input
                            historyIndex = -1;
                            input = originalInput;
                            cursorPosition = input.Length;
                            DrawLine(input, cursorPosition, options);
                        }
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (IsCtrlKeyPressed(key))
                    {
                        // go to start of word
                        cursorPosition = Functions.FindWordStart(input, cursorPosition);
                        DrawLine(input, cursorPosition, options);
                    }
                    else if (cursorPosition > 0)
                    {
                        cursorPosition--;
                        DrawLine(input, cursorPosition, options);
                    }
                    break;
                case ConsoleKey.RightArrow:
                    if (IsCtrlKeyPressed(key))
                    {
                        // go to end of word
                        cursorPosition = Functions.FindWordEnd(input, cursorPosition);
                        DrawLine(input, cursorPosition, options);
                    }
                    else if (cursorPosition < input.Length)
                    {
                        cursorPosition++;
                        DrawLine(input, cursorPosition, options);
                    }
                    break;
                case ConsoleKey.Home:
                    cursorPosition = 0;
                    DrawLine(input, cursorPosition, options);
                    break;
                case ConsoleKey.End:
                    cursorPosition = input.Length;
                    DrawLine(input, cursorPosition, options);
                    break;
                case ConsoleKey.U:
                case ConsoleKey.Z:
                    if (IsCtrlKeyPressed(key))
                    {
                        // Undo (Ctrl+U is safer than Ctrl+Z on Unix systems)
                        if (undoStack.Count > 0)
                        {
                            // Save current state to redo stack
                            redoStack.Push(new InputState(input, cursorPosition));

                            // Restore previous state
                            var previousState = undoStack.Pop();
                            input = previousState.Input;
                            cursorPosition = Math.Min(previousState.CursorPosition, input.Length);

                            // Update originalInput if we're not in history navigation mode
                            if (historyIndex == -1)
                            {
                                originalInput = input;
                            }

                            DrawLine(input, cursorPosition, options);
                        }
                    }
                    break;
                case ConsoleKey.Y:
                    if (IsCtrlKeyPressed(key))
                    {
                        if (redoStack.Count > 0)
                        {
                            // Save current state to undo stack
                            undoStack.Push(new InputState(input, cursorPosition));

                            // Restore next state
                            var nextState = redoStack.Pop();
                            input = nextState.Input;
                            cursorPosition = Math.Min(nextState.CursorPosition, input.Length);

                            // Update originalInput if we're not in history navigation mode
                            if (historyIndex == -1)
                            {
                                originalInput = input;
                            }

                            DrawLine(input, cursorPosition, options);
                        }
                    }
                    break;
                case ConsoleKey.W:
                    if (IsCtrlKeyPressed(key))
                    {
                        // Save state for undo before modification
                        SaveStateForUndo();

                        // Reset history navigation when editing
                        historyIndex = -1;

                        // Delete word to the left
                        int wordStart = Functions.FindWordStart(input, cursorPosition);

                        if (wordStart < cursorPosition)
                        {
                            input = input.Remove(wordStart, cursorPosition - wordStart);
                            cursorPosition = wordStart;
                            DrawLine(input, cursorPosition, options);
                        }
                    }
                    break;
                case ConsoleKey.Tab:
                    if (_enableAutoComplete && options.AutoCompleteItems.Count > 0)
                    {
                        // Save state for undo before modification
                        SaveStateForUndo();

                        // Reset history navigation when using autocomplete
                        historyIndex = -1;

                        // Get current word to autocomplete
                        string currentWord = Functions.GetCurrentWord(input, cursorPosition);

                        if (currentWord.Length >= options.AutoCompleteMinLength)
                        {
                            var comparison = options.AutoCompleteCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                            var matches = options.AutoCompleteItems
                                .Where(item => item.StartsWith(currentWord, comparison))
                                .ToList();

                            if (matches.Count > 0)
                            {
                                // Use the first match for autocomplete
                                string completion = matches[0];

                                // Find the bounds of the entire word we're in
                                int wordStart = cursorPosition - currentWord.Length;
                                int wordEnd = Functions.FindWordEnd(input, cursorPosition);

                                // Replace the entire word (both the typed part and any remaining characters)
                                int entireWordLength = wordEnd - wordStart;
                                input = input.Remove(wordStart, entireWordLength);
                                input = input.Insert(wordStart, completion);
                                cursorPosition = wordStart + completion.Length;

                                DrawLine(input, cursorPosition, options);
                            }
                        }
                    }
                    break;

                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        // Save state for undo before modification
                        SaveStateForUndo();

                        // Reset history navigation when typing new characters
                        historyIndex = -1;

                        input = input.Insert(cursorPosition, key.KeyChar.ToString());
                        cursorPosition++;
                        DrawLine(input, cursorPosition, options);
                    }
                    break;
            }

            if (options.EnableDebug)
                Console.Write($"Key pressed: {key.KeyChar}, Key: {key.Key}, Control: {key.Modifiers}");

        } while (true);
    }

    private static void DrawLine(string input, int cursorDelPosition, JReadOptions options)
    {
        var displayInfo = Construct.ConstructDisplayString(input, cursorDelPosition, options);
        Writer.RenderDisplayString(displayInfo, options);
    }
}