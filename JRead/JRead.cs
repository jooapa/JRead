namespace JRead;

public class JReadOptions
{
    public string[]? History { get; set; } = null;
    public bool AddPrefillToHistory { get; set; } = false;
    public bool EnableDebug { get; set; } = false;
}

public class JRead
{
    public string ReadLine(string? preFill = null, JReadOptions? options = null)
    {
        return ReadLineWithEscapeDetectionAndPrefill(preFill, options);
    }

    private string ReadLineWithEscapeDetectionAndPrefill(string? prefillText, JReadOptions? options = null)
    {
        bool IsCtrlKeyPressed(ConsoleKeyInfo keyInfo) => (keyInfo.Modifiers & ConsoleModifiers.Control) != 0;
        bool IsShiftKeyPressed(ConsoleKeyInfo keyInfo) => (keyInfo.Modifiers & ConsoleModifiers.Shift) != 0;
        bool IsAltKeyPressed(ConsoleKeyInfo keyInfo) => (keyInfo.Modifiers & ConsoleModifiers.Alt) != 0;

        string input = prefillText ?? "";
        ConsoleKeyInfo key;
        int cursorPosition = input.Length;
        int suggestionIndex = -1; // -1 = user input, 0+ = suggestion index
        string originalInput = prefillText ?? "";

        // Track rendering info for consistent multi-line handling
        int startingCursorLeft = Console.CursorLeft;
        int startingCursorTop = Console.CursorTop;
        int maxNewlines = input.Count(c => c == '\n'); // Track max newlines seen

        // set options stuff ---
        if (options != null)
        {
            if (options.History != null)
            {
                if (options.AddPrefillToHistory)
                {
                    // add the prefill also to history
                    if (!string.IsNullOrEmpty(prefillText))
                    {
                        var historyList = options.History.ToList();
                        historyList.Add(prefillText);
                        options.History = [.. historyList];
                    }
                }
            }
        }
        else
        {
            options = new JReadOptions();
        }

        // Initial render of the input
        Console.Write(input);
        
        // Update max newlines after initial render
        maxNewlines = Math.Max(maxNewlines, input.Count(c => c == '\n'));

        do
        {
            key = Console.ReadKey(true);

            
            if (options.EnableDebug)
                Console.WriteLine($"\n[DEBUG] Key: {key.Key}, Char: '{key.KeyChar}', Modifiers: {key.Modifiers}, CursorPos: {cursorPosition}, Input: '{input}', SuggestionIndex: {suggestionIndex}");

            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    ClearInputLine(maxNewlines, startingCursorLeft, startingCursorTop);
                    return string.Empty;

                case ConsoleKey.Backspace:
                    if (input.Length > 0 && cursorPosition > 0)
                    {
                        ResetSuggestionIndex(ref suggestionIndex, ref originalInput, input);

                        // Remove single character before cursor
                        input = input.Substring(0, cursorPosition - 1) + input.Substring(cursorPosition);
                        cursorPosition--;

                        // Update max newlines if necessary
                        maxNewlines = Math.Max(maxNewlines, input.Count(c => c == '\n'));

                        RedrawInputLine(input, cursorPosition, maxNewlines, startingCursorLeft, startingCursorTop);
                    }
                    break;

                case ConsoleKey.W:
                    // Handle Ctrl+W (which is often Ctrl+Backspace in terminals)
                    if (IsCtrlKeyPressed(key))
                    {
                        if (input.Length > 0 && cursorPosition > 0)
                        {
                            ResetSuggestionIndex(ref suggestionIndex, ref originalInput, input);

                            // Local function to find the start of the current word
                            int FindWordStart(string text, int position)
                            {
                                if (position <= 0) return 0;

                                int pos = position - 1;

                                // Skip whitespace first
                                while (pos >= 0 && char.IsWhiteSpace(text[pos]))
                                    pos--;

                                // Then skip the word characters
                                while (pos >= 0 && !char.IsWhiteSpace(text[pos]))
                                    pos--;

                                return pos + 1;
                            }

                            // Delete entire word
                            int wordStart = FindWordStart(input, cursorPosition);
                            int charactersToDelete = cursorPosition - wordStart;

                            if (charactersToDelete > 0)
                            {
                                input = input.Substring(0, wordStart) + input.Substring(cursorPosition);
                                cursorPosition = wordStart;
                            }

                            // Update max newlines if necessary
                            maxNewlines = Math.Max(maxNewlines, input.Count(c => c == '\n'));

                            RedrawInputLine(input, cursorPosition, maxNewlines, startingCursorLeft, startingCursorTop);
                        }
                    }
                    else
                    {
                        // If not Ctrl+W, treat as regular character input
                        goto default;
                    }
                    break;

                case ConsoleKey.Delete:
                    if (cursorPosition < input.Length)
                    {
                        ResetSuggestionIndex(ref suggestionIndex, ref originalInput, input);

                        // Remove character at cursor position
                        input = input.Substring(0, cursorPosition) + input.Substring(cursorPosition + 1);

                        // Update max newlines if necessary
                        maxNewlines = Math.Max(maxNewlines, input.Count(c => c == '\n'));

                        RedrawInputLine(input, cursorPosition, maxNewlines, startingCursorLeft, startingCursorTop);
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (cursorPosition > 0)
                    {
                        cursorPosition--;
                        
                        // Just redraw the entire input to ensure cursor is positioned correctly
                        RedrawInputLine(input, cursorPosition, maxNewlines, startingCursorLeft, startingCursorTop);
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if (cursorPosition < input.Length)
                    {
                        cursorPosition++;
                        
                        // Just redraw the entire input to ensure cursor is positioned correctly
                        RedrawInputLine(input, cursorPosition, maxNewlines, startingCursorLeft, startingCursorTop);
                    }
                    break;

                case ConsoleKey.Home:
                    // Move cursor to beginning of input
                    if (cursorPosition > 0)
                    {
                        cursorPosition = 0;
                        RedrawInputLine(input, cursorPosition, maxNewlines, startingCursorLeft, startingCursorTop);
                    }
                    break;

                case ConsoleKey.End:
                    // Move cursor to end of input
                    if (cursorPosition < input.Length)
                    {
                        cursorPosition = input.Length;
                        RedrawInputLine(input, cursorPosition, maxNewlines, startingCursorLeft, startingCursorTop);
                    }
                    break;

                case ConsoleKey.UpArrow:
                    // Navigate through options.History upward
                    if (options.History != null && options.History.Length > 0)
                    {
                        // If currently on user input (-1), save it as original
                        if (suggestionIndex == -1)
                        {
                            originalInput = input;
                        }

                        suggestionIndex++;
                        if (suggestionIndex >= options.History.Length)
                        {
                            suggestionIndex = 0; // Wrap to first suggestion
                        }

                        ReplaceInputField(options.History[suggestionIndex], ref input, ref cursorPosition, ref maxNewlines, startingCursorLeft, startingCursorTop);
                    }
                    break;

                case ConsoleKey.DownArrow:
                    // Navigate through options.History downward
                    if (options.History != null && options.History.Length > 0)
                    {
                        // If currently on user input (-1), save it as original
                        if (suggestionIndex == -1)
                        {
                            originalInput = input;
                        }

                        suggestionIndex--;
                        if (suggestionIndex < -1)
                        {
                            suggestionIndex = options.History.Length - 1; // Wrap to last suggestion
                        }

                        if (suggestionIndex == -1)
                        {
                            // Restore original user input
                            ReplaceInputField(originalInput, ref input, ref cursorPosition, ref maxNewlines, startingCursorLeft, startingCursorTop);
                        }
                        else
                        {
                            ReplaceInputField(options.History[suggestionIndex], ref input, ref cursorPosition, ref maxNewlines, startingCursorLeft, startingCursorTop);
                        }
                    }
                    break;

                case ConsoleKey.Enter:
                    Console.WriteLine();
                    return input;

                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        ResetSuggestionIndex(ref suggestionIndex, ref originalInput, input);

                        // Insert character at cursor position
                        input = input.Substring(0, cursorPosition) + key.KeyChar + input.Substring(cursorPosition);
                        cursorPosition++;

                        // Update max newlines if necessary
                        maxNewlines = Math.Max(maxNewlines, input.Count(c => c == '\n'));

                        RedrawInputLine(input, cursorPosition, maxNewlines, startingCursorLeft, startingCursorTop);
                    }
                    break;
            }
        } while (true);
    }

    private static void ResetSuggestionIndex(ref int suggestionIndex, ref string originalInput, string currentInput)
    {
        if (suggestionIndex != -1)
        {
            suggestionIndex = -1;
            originalInput = currentInput;
        }
    }

    private static void RedrawInputLine(string input, int cursorPosition, int maxNewlines, int startingLeft, int startingTop)
    {
        // Bounds checking for starting position
        if (startingTop < 0 || startingTop >= Console.BufferHeight || startingLeft < 0 || startingLeft >= Console.WindowWidth)
        {
            return; // Can't draw if starting position is invalid
        }
        
        // Calculate how many lines are available from the starting position
        int availableLines = Console.BufferHeight - startingTop;
        
        // Split input into lines to see what we're working with
        var inputLines = input.Split('\n');
        int inputLinesCount = inputLines.Length;
        
        // Determine how many lines we can safely clear and draw
        int linesToClear = Math.Min(maxNewlines + 1, availableLines);
        int linesToDraw = Math.Min(inputLinesCount, availableLines);
        
        // Go to starting position
        Console.CursorLeft = startingLeft;
        Console.CursorTop = startingTop;
        
        // Clear the lines we might have used (but only what fits)
        for (int i = 0; i < linesToClear; i++)
        {
            int clearWidth = Math.Min(Console.WindowWidth - startingLeft, Console.WindowWidth - 1);
            Console.Write(new string(' ', clearWidth));
            if (i < linesToClear - 1 && Console.CursorTop + 1 < Console.BufferHeight)
            {
                Console.WriteLine();
            }
        }
        
        // Go back to starting position
        Console.CursorLeft = startingLeft;
        Console.CursorTop = startingTop;
        
        // Write the input, but only as much as fits in available buffer space
        if (inputLinesCount <= availableLines)
        {
            // All input fits, write it normally
            Console.Write(input);
        }
        else
        {
            // Input is too tall, write what we can
            var linesToWrite = inputLines.Take(availableLines - 1).ToArray();
            string visibleInput = string.Join('\n', linesToWrite);
            if (availableLines > 1)
            {
                visibleInput += "\n[...]"; // Show truncation indicator if we have space
            }
            Console.Write(visibleInput);
        }
        
        // Position cursor correctly
        if (cursorPosition <= input.Length)
        {
            // Calculate where cursor should be based on the full input
            string beforeCursor = input.Substring(0, Math.Min(cursorPosition, input.Length));
            var cursorLines = beforeCursor.Split('\n');
            int targetLine = cursorLines.Length - 1;
            int targetColumn = startingLeft + cursorLines[cursorLines.Length - 1].Length;
            
            int newTop = startingTop + targetLine;
            
            // Make sure cursor position is within available buffer space
            if (newTop >= startingTop && newTop < Console.BufferHeight && 
                targetColumn >= 0 && targetColumn < Console.WindowWidth &&
                targetLine < availableLines)
            {
                Console.CursorTop = newTop;
                Console.CursorLeft = targetColumn;
            }
            else
            {
                // Cursor would be outside visible area, put it at end of visible content
                int lastVisibleLine = Math.Min(linesToDraw - 1, availableLines - 1);
                Console.CursorTop = startingTop + lastVisibleLine;
                
                if (lastVisibleLine < inputLines.Length)
                {
                    // Position at end of the last visible line
                    Console.CursorLeft = startingLeft + inputLines[lastVisibleLine].Length;
                }
                else
                {
                    Console.CursorLeft = startingLeft;
                }
            }
        }
    }

    private static void ClearInputLine(int maxNewlines, int startingLeft, int startingTop)
    {
        // Bounds checking for starting position
        if (startingTop < 0 || startingTop >= Console.BufferHeight || startingLeft < 0 || startingLeft >= Console.WindowWidth)
        {
            return; // Can't clear if starting position is invalid
        }
        
        // Clear all lines we might have used
        int totalLines = maxNewlines + 1;
        int maxClearableLines = Math.Min(totalLines, Console.BufferHeight - startingTop);
        
        // Go to starting position
        Console.CursorLeft = startingLeft;
        Console.CursorTop = startingTop;
        
        // Clear all lines
        for (int i = 0; i < maxClearableLines; i++)
        {
            Console.Write(new string(' ', Math.Min(Console.WindowWidth - 1, Console.WindowWidth - startingLeft)));
            if (i < maxClearableLines - 1 && Console.CursorTop + 1 < Console.BufferHeight)
            {
                Console.WriteLine();
            }
        }
        
        // Return to starting position
        if (startingTop < Console.BufferHeight && startingLeft < Console.WindowWidth)
        {
            Console.CursorLeft = startingLeft;
            Console.CursorTop = startingTop;
        }
    }

    private static void ReplaceInputField(string newText, ref string input, ref int cursorPosition, ref int maxNewlines, int startingLeft, int startingTop)
    {
        ClearInputLine(maxNewlines, startingLeft, startingTop);

        // Write new text
        input = newText ?? "";
        
        // Update max newlines
        maxNewlines = Math.Max(maxNewlines, input.Count(c => c == '\n'));
        
        Console.Write(input);

        // Set cursor to end of new input
        cursorPosition = input.Length;
    }
}
