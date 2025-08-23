namespace JRead;

public class JReadOptions
{
    public string[]? Suggestions { get; set; } = null;
}

public class JRead
{
    public string ReadLine(string? preFill = null, JReadOptions? options = null)
    {
        return ReadLineWithEscapeDetectionAndPrefill(preFill, options?.Suggestions);
    }

    private string ReadLineWithEscapeDetectionAndPrefill(string? prefillText, string[]? suggestions = null)
    {
        bool IsCtrlKeyPressed(ConsoleKeyInfo keyInfo) => (keyInfo.Modifiers & ConsoleModifiers.Control) != 0;
        bool IsShiftKeyPressed(ConsoleKeyInfo keyInfo) => (keyInfo.Modifiers & ConsoleModifiers.Shift) != 0;
        bool IsAltKeyPressed(ConsoleKeyInfo keyInfo) => (keyInfo.Modifiers & ConsoleModifiers.Alt) != 0;
        bool IsNoneModKeyPressed(ConsoleKeyInfo keyInfo) => (keyInfo.Modifiers & (ConsoleModifiers.Control | ConsoleModifiers.Shift | ConsoleModifiers.Alt)) == 0;

        string input = prefillText ?? "";
        ConsoleKeyInfo keyInfo;
        int cursorPosition = input.Length;
        int suggestionIndex = -1; // -1 = user input, 0+ = suggestion index
        string originalInput = prefillText ?? "";

        Console.Write(input);

        do
        {
            keyInfo = Console.ReadKey(true);


            if (keyInfo.Key == ConsoleKey.Escape)
            {
                // Clear the current input line and return empty
                Console.Write(new string('\b', input.Length));
                Console.Write(new string(' ', input.Length));
                Console.Write(new string('\b', input.Length));
                return string.Empty;
            }
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (input.Length > 0 && cursorPosition > 0)
                {
                    // Reset suggestion index when user starts typing
                    if (suggestionIndex != -1)
                    {
                        suggestionIndex = -1;
                        originalInput = input;
                    }

                    // Remove character before cursor
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
            }
            else if (keyInfo.Key == ConsoleKey.Delete)
            {
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
            }
            else if (keyInfo.Key == ConsoleKey.LeftArrow)
            {
                if (cursorPosition > 0)
                {
                    cursorPosition--;
                    Console.Write('\b');
                }
            }
            else if (keyInfo.Key == ConsoleKey.RightArrow)
            {
                if (cursorPosition < input.Length)
                {
                    cursorPosition++;
                    Console.Write(input[cursorPosition - 1]);
                }
            }
            else if (keyInfo.Key == ConsoleKey.Home)
            {
                // Move cursor to beginning of input
                if (cursorPosition > 0)
                {
                    Console.Write(new string('\b', cursorPosition));
                    cursorPosition = 0;
                }
            }
            else if (keyInfo.Key == ConsoleKey.End)
            {
                // Move cursor to end of input
                if (cursorPosition < input.Length)
                {
                    Console.Write(input.Substring(cursorPosition));
                    cursorPosition = input.Length;
                }
            }
            else if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                // Navigate through suggestions upward
                if (suggestions != null && suggestions.Length > 0)
                {
                    // If currently on user input (-1), save it as original
                    if (suggestionIndex == -1)
                    {
                        originalInput = input;
                    }

                    suggestionIndex++;
                    if (suggestionIndex >= suggestions.Length)
                    {
                        suggestionIndex = 0; // Wrap to first suggestion
                    }

                    ReplaceInputField(suggestions[suggestionIndex], ref input, ref cursorPosition);
                }
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                // Navigate through suggestions downward
                if (suggestions != null && suggestions.Length > 0)
                {
                    // If currently on user input (-1), save it as original
                    if (suggestionIndex == -1)
                    {
                        originalInput = input;
                    }

                    suggestionIndex--;
                    if (suggestionIndex < -1)
                    {
                        suggestionIndex = suggestions.Length - 1; // Wrap to last suggestion
                    }

                    if (suggestionIndex == -1)
                    {
                        // Restore original user input
                        ReplaceInputField(originalInput, ref input, ref cursorPosition);
                    }
                    else
                    {
                        ReplaceInputField(suggestions[suggestionIndex], ref input, ref cursorPosition);
                    }
                }
            }
            else if (keyInfo.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                // Reset suggestion index when user starts typing
                if (suggestionIndex != -1)
                {
                    suggestionIndex = -1;
                    originalInput = input;
                }

                // Insert character at cursor position
                input = input.Substring(0, cursorPosition) + keyInfo.KeyChar + input.Substring(cursorPosition);
                cursorPosition++;

                // Redraw from cursor position
                Console.Write(input.Substring(cursorPosition - 1));

                // Position cursor correctly
                int moveBack = input.Length - cursorPosition;
                if (moveBack > 0)
                    Console.Write(new string('\b', moveBack));
            }
        } while (true);

        return input;
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
