using System;
using System.Collections.Generic;
using System.Linq;

namespace JRead;

public static class JRead
{
    public static JReadHistory History { get; } = new();
    private static readonly char[] AutocompleteWordBoundaries = { ' ', '"', '\'', '/', '(', ')', '[', ']', '{', '}', ',', '.', ';', ':', '!', '?', '@', '#', '$', '%', '^', '&', '*', '+', '=', '|', '\\', '<', '>', '~', '`' };

    // maybe i should not change options globally with changes
    private static bool _enableAutoComplete = false;

    /// <summary>
    /// Reads a line, but if EscapingReturnsTheOriginalInput is false, and escaping. will function return null. 
    /// </summary>
    /// <param name="prefillText"></param>
    /// <param name="beginningText"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string? Read(string? prefillText = null, string beginningText = "", JReadOptions? options = null)
    {
        return ReadInternal(prefillText, options, beginningText);
    }

    /// <summary>
    /// Reads a line, but if EscapingReturnsTheOriginalInput is false, and escaping. will function return null. 
    /// </summary>
    /// <param name="prefillText"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string? Read(string? prefillText, JReadOptions options)
    {
        return ReadInternal(prefillText, options, "");
    }

    /// <summary>
    /// Reads a line, but will not return null.
    /// </summary>
    /// <param name="prefillText"></param>
    /// <param name="beginningText"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string ReadNoNull(string? prefillText = null, string beginningText = "", JReadOptions? options = null)
    {
        return ReadInternal(prefillText, options, beginningText) ?? string.Empty;
    }

    /// <summary>
    /// Reads a line, but will not return null.
    /// </summary>
    /// <param name="prefillText"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string ReadNoNull(string? prefillText, JReadOptions options)
    {
        return ReadInternal(prefillText, options, "") ?? string.Empty;
    }

    private static string? ReadInternal(string? prefillText, JReadOptions? options = null, string beginningText = "")
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
        JReadHistory history = options.CustomHistory ?? History;

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
                    return null;

                case ConsoleKey.Enter:
                    // Add to history if enabled
                    if (options.AddReturnedValueToHistory)
                    {
                        history.Add(input);
                    }
                    // Console.WriteLine();
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
                        cursorPosition = FindWordStart(input, cursorPosition);
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
                        cursorPosition = FindWordEnd(input, cursorPosition);
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
                        int wordStart = FindWordStart(input, cursorPosition);

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
                        string currentWord = GetCurrentWord(input, cursorPosition);

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
                                int wordEnd = FindWordEnd(input, cursorPosition);

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

    private static void SafeSetCursorPosition(int left, int top)
    {
        // Ensure coordinates are within valid bounds
        left = Math.Max(0, Math.Min(left, Console.WindowWidth - 1));
        top = Math.Max(0, Math.Min(top, Console.WindowHeight - 1));
        
        try
        {
            Console.SetCursorPosition(left, top);
        }
        catch (ArgumentOutOfRangeException)
        {
            // If still fails, try to position at origin
            try
            {
                Console.SetCursorPosition(0, Math.Max(0, Console.CursorTop));
            }
            catch
            {
                // Last resort - do nothing
            }
        }
    }

    private static void DrawLine(string input, int cursorDelPosition, JReadOptions options)
    {
        CursorPos originalPos = options._cursorPos;

        // Convert newlines to visible characters for display
        string ConvertNewlinesToVisible(string text)
        {
            return text.Replace("\n", "↵").Replace("\r", "");
        }

        // Get autocomplete suggestion if enabled
        string autoCompleteSuggestion = "";
        if (_enableAutoComplete && options.AutoCompleteItems.Count > 0)
        {
            string currentWord = GetCurrentWord(input, cursorDelPosition);
            if (currentWord.Length >= options.AutoCompleteMinLength)
            {
                // Only show suggestions if we're at the end of the current word
                bool atEndOfWord = IsAtEndOfWord(input, cursorDelPosition);

                if (atEndOfWord)
                {
                    var comparison = options.AutoCompleteCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                    var match = options.AutoCompleteItems
                        .FirstOrDefault(item => item.StartsWith(currentWord, comparison));

                    if (match != null && match.Length > currentWord.Length)
                    {
                        autoCompleteSuggestion = match.Substring(currentWord.Length);
                    }
                }
            }
        }

        // Convert input to visible format
        string visibleInput = ConvertNewlinesToVisible(input);
        
        // Calculate available space from original position to end of line
        int availableWidth = Console.WindowWidth - originalPos.Left;
        
        // Clear the current line from the original position
        SafeSetCursorPosition(originalPos.Left, originalPos.Top);
        if (availableWidth > 0)
        {
            Console.Write(new string(' ', availableWidth));
        }
        
        // Return to original position
        SafeSetCursorPosition(originalPos.Left, originalPos.Top);
        
        // If text fits in available space, show everything
        int totalLength = visibleInput.Length + autoCompleteSuggestion.Length;
        if (totalLength <= availableWidth)
        {
            // Split input at cursor position for proper cursor placement
            string beforeCursor = ConvertNewlinesToVisible(input.Substring(0, cursorDelPosition));
            string afterCursor = ConvertNewlinesToVisible(input.Substring(cursorDelPosition));

            // Write text before cursor
            if (options.EnableMaskedInput)
                Console.Write(new string(options.MaskedInputChar, beforeCursor.Length));
            else
                Console.Write(beforeCursor);

            // Save cursor position
            int cursorLeft = Console.CursorLeft;
            int cursorTop = Console.CursorTop;
            
            // Write autocomplete suggestion in gray
            if (!string.IsNullOrEmpty(autoCompleteSuggestion))
            {
                ConsoleColor originalFg = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(autoCompleteSuggestion);
                Console.ForegroundColor = originalFg;
            }
            
            // Write text after cursor
            if (options.EnableMaskedInput)
                Console.Write(new string(options.MaskedInputChar, afterCursor.Length));
            else
                Console.Write(afterCursor);

            // Position cursor at the right place with bounds checking
            SafeSetCursorPosition(cursorLeft, cursorTop);
        }
        else if (availableWidth > 6) // Need at least 6 characters for meaningful truncation with "..."
        {
            // Text doesn't fit, need to truncate intelligently
            string beforeCursor = ConvertNewlinesToVisible(input.Substring(0, cursorDelPosition));
            string afterCursor = ConvertNewlinesToVisible(input.Substring(cursorDelPosition));
            
            // Calculate how much space to show before and after cursor
            int spaceForEllipsis = 3; // "..." takes 3 characters
            int spaceForContent = availableWidth - spaceForEllipsis;
            
            // Try to show some context before and after cursor
            int beforeCursorLength = Math.Min(beforeCursor.Length, spaceForContent / 2);
            int afterCursorLength = Math.Min(afterCursor.Length, spaceForContent - beforeCursorLength);
            
            // Adjust if we have extra space
            if (beforeCursorLength + afterCursorLength < spaceForContent)
            {
                if (beforeCursor.Length > beforeCursorLength)
                {
                    beforeCursorLength = Math.Min(beforeCursor.Length, spaceForContent - afterCursorLength);
                }
                else if (afterCursor.Length > afterCursorLength)
                {
                    afterCursorLength = Math.Min(afterCursor.Length, spaceForContent - beforeCursorLength);
                }
            }
            
            // Determine what to show
            bool showStartEllipsis = cursorDelPosition > beforeCursorLength;
            bool showEndEllipsis = (cursorDelPosition + afterCursorLength) < visibleInput.Length;
            
            // Build the display string
            string displayText = "";
            int displayCursorPos = 0;
            
            if (showStartEllipsis)
            {
                displayText += "...";
                displayCursorPos = 3;
            }
            
            // Add text before cursor
            if (beforeCursorLength > 0)
            {
                int startIndex = showStartEllipsis ? 
                    Math.Max(0, cursorDelPosition - beforeCursorLength) : 0;
                string textBefore = beforeCursor.Substring(Math.Max(0, beforeCursor.Length - beforeCursorLength));
                displayText += textBefore;
                displayCursorPos += textBefore.Length;
            }
            
            // Add text after cursor
            if (afterCursorLength > 0)
            {
                string textAfter = afterCursor.Substring(0, Math.Min(afterCursor.Length, afterCursorLength));
                displayText += textAfter;
            }
            
            if (showEndEllipsis)
            {
                displayText += "...";
            }

            // Write the truncated text
            if (options.EnableMaskedInput)
                Console.Write(new string(options.MaskedInputChar, Math.Min(displayText.Length, availableWidth)));
            else
                Console.Write(displayText.Substring(0, Math.Min(displayText.Length, availableWidth)));
            
            // Position cursor correctly with bounds checking
            SafeSetCursorPosition(originalPos.Left + displayCursorPos, originalPos.Top);
        }
        else
        {
            // Terminal too narrow, just show cursor position indicator
            string indicator = cursorDelPosition.ToString();
            if (indicator.Length <= availableWidth)
            {
                Console.Write(indicator);
                SafeSetCursorPosition(originalPos.Left + indicator.Length, originalPos.Top);
            }
        }
    }

    private static string GetCurrentWord(string input, int cursorPosition)
    {
        if (string.IsNullOrEmpty(input) || cursorPosition <= 0)
            return string.Empty;

        // Find the start of the current word
        int wordStart = FindWordStart(input, cursorPosition);

        // Get only the part of the word that's before the cursor (what we've typed so far)
        int length = cursorPosition - wordStart;
        if (length <= 0)
            return string.Empty;

        return input.Substring(wordStart, length);
    }

    /// <summary>
    /// Finds the start of a word by looking backwards from the given position
    /// Handles spaces intelligently for word navigation
    /// </summary>
    private static int FindWordStart(string input, int position)
    {
        if (position <= 0) return 0;
        
        // Start from the character before the cursor
        int current = position - 1;
        
        // If we're starting on a word boundary (like space), skip backwards through word boundaries
        if (current >= 0 && Array.IndexOf(AutocompleteWordBoundaries, input[current]) != -1)
        {
            // Skip backwards through word boundaries (spaces, punctuation, etc.)
            while (current >= 0 && Array.IndexOf(AutocompleteWordBoundaries, input[current]) != -1)
            {
                current--;
            }
        }
        
        // Now skip backwards through the word characters to find the start
        while (current >= 0 && Array.IndexOf(AutocompleteWordBoundaries, input[current]) == -1)
        {
            current--;
        }
        
        return current + 1; // Move to the first character of the word
    }

    /// <summary>
    /// Finds the end of a word by looking forwards from the given position
    /// Handles spaces intelligently for word navigation
    /// </summary>
    private static int FindWordEnd(string input, int position)
    {
        if (position >= input.Length) return input.Length;
        
        int current = position;
        
        // If we're starting on a word boundary (like space), skip forward through word boundaries
        if (current < input.Length && Array.IndexOf(AutocompleteWordBoundaries, input[current]) != -1)
        {
            // Skip forward through word boundaries (spaces, punctuation, etc.)
            while (current < input.Length && Array.IndexOf(AutocompleteWordBoundaries, input[current]) != -1)
            {
                current++;
            }
        }
        
        // Now skip forward through the word characters to find the end
        while (current < input.Length && Array.IndexOf(AutocompleteWordBoundaries, input[current]) == -1)
        {
            current++;
        }
        
        return current;
    }

    /// <summary>
    /// Checks if the cursor is at the end of a word or at a word boundary
    /// </summary>
    private static bool IsAtEndOfWord(string input, int position)
    {
        return position >= input.Length || Array.IndexOf(AutocompleteWordBoundaries, input[position]) != -1;
    }
}
