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

        Console.Write(input);

        do
        {
            key = Console.ReadKey(true);

            
            if (options.EnableDebug)
                Console.WriteLine($"\n[DEBUG] Key: {key.Key}, Char: '{key.KeyChar}', Modifiers: {key.Modifiers}, CursorPos: {cursorPosition}, Input: '{input}', SuggestionIndex: {suggestionIndex}");

            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    // Clear the current input line and return empty
                    Console.Write(new string('\b', input.Length));
                    Console.Write(new string(' ', input.Length));
                    Console.Write(new string('\b', input.Length));
                    return string.Empty;

                case ConsoleKey.Backspace:
                    if (input.Length > 0 && cursorPosition > 0)
                    {
                        // Reset suggestion index when user starts typing
                        if (suggestionIndex != -1)
                        {
                            suggestionIndex = -1;
                            originalInput = input;
                        }

                        // Remove single character before cursor
                        input = input.Substring(0, cursorPosition - 1) + input.Substring(cursorPosition);
                        cursorPosition--;

                        // Redraw the line
                        Console.Write(new string('\b', input.Length + 1));
                        Console.Write(new string(' ', input.Length + 1));
                        Console.Write(new string('\b', input.Length + 1));
                        Console.Write(input);

                        // Position cursor correctly
                        int moveBack = input.Length - cursorPosition;
                        if (moveBack > 0)
                            Console.Write(new string('\b', moveBack));
                    }
                    break;

                case ConsoleKey.W:
                    // Handle Ctrl+W (which is often Ctrl+Backspace in terminals)
                    if (IsCtrlKeyPressed(key))
                    {
                        if (input.Length > 0 && cursorPosition > 0)
                        {
                            // Reset suggestion index when user starts typing
                            if (suggestionIndex != -1)
                            {
                                suggestionIndex = -1;
                                originalInput = input;
                            }

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

                            // Redraw the line
                            Console.Write(new string('\b', input.Length + charactersToDelete));
                            Console.Write(new string(' ', input.Length + charactersToDelete));
                            Console.Write(new string('\b', input.Length + charactersToDelete));
                            Console.Write(input);

                            // Position cursor correctly
                            int moveBack = input.Length - cursorPosition;
                            if (moveBack > 0)
                                Console.Write(new string('\b', moveBack));
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
                        // Reset suggestion index when user starts typing
                        if (suggestionIndex != -1)
                        {
                            suggestionIndex = -1;
                            originalInput = input;
                        }

                        // Remove character at cursor position
                        input = input.Substring(0, cursorPosition) + input.Substring(cursorPosition + 1);

                        // Redraw the line
                        Console.Write(new string('\b', cursorPosition));
                        Console.Write(input);
                        Console.Write(" "); // Clear the last character

                        // Position cursor correctly
                        int moveBack = input.Length - cursorPosition + 1;
                        Console.Write(new string('\b', moveBack));
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (cursorPosition > 0)
                    {
                        cursorPosition--;
                        Console.Write('\b');
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if (cursorPosition < input.Length)
                    {
                        cursorPosition++;
                        Console.Write(input[cursorPosition - 1]);
                    }
                    break;

                case ConsoleKey.Home:
                    // Move cursor to beginning of input
                    if (cursorPosition > 0)
                    {
                        Console.Write(new string('\b', cursorPosition));
                        cursorPosition = 0;
                    }
                    break;

                case ConsoleKey.End:
                    // Move cursor to end of input
                    if (cursorPosition < input.Length)
                    {
                        Console.Write(input.Substring(cursorPosition));
                        cursorPosition = input.Length;
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

                        ReplaceInputField(options.History[suggestionIndex], ref input, ref cursorPosition);
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
                            ReplaceInputField(originalInput, ref input, ref cursorPosition);
                        }
                        else
                        {
                            ReplaceInputField(options.History[suggestionIndex], ref input, ref cursorPosition);
                        }
                    }
                    break;

                case ConsoleKey.Enter:
                    Console.WriteLine();
                    return input;

                default:
                    if (!char.IsControl(key.KeyChar))
                    {
                        // Reset suggestion index when user starts typing
                        if (suggestionIndex != -1)
                        {
                            suggestionIndex = -1;
                            originalInput = input;
                        }

                        // Insert character at cursor position
                        input = input.Substring(0, cursorPosition) + key.KeyChar + input.Substring(cursorPosition);
                        cursorPosition++;

                        // Redraw from cursor position
                        Console.Write(input.Substring(cursorPosition - 1));

                        // Position cursor correctly
                        int moveBack = input.Length - cursorPosition;
                        if (moveBack > 0)
                            Console.Write(new string('\b', moveBack));
                    }
                    break;
            }
        } while (true);
    }

    private static void ReplaceInputField(string newText, ref string input, ref int cursorPosition)
    {
        // Clear current input from console
        Console.Write(new string('\b', input.Length));
        Console.Write(new string(' ', input.Length));
        Console.Write(new string('\b', input.Length));

        // Write new text
        input = newText ?? "";
        Console.Write(input);

        // Set cursor to end of new input
        cursorPosition = input.Length;
    }
}
