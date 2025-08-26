using System;
using System.Collections.Generic;

namespace JRead;

public static class JRead
{
    // Global history instance for easy access
    public static JReadHistory History { get; } = new();

    // Word boundary characters used throughout the application
    private static readonly char[] WordBoundaries = { ' ', '"', '\'', '/', '(', ')', '[', ']', '{', '}', ',', '.', ';', ':', '!', '?', '@', '#', '$', '%', '^', '&', '*', '+', '=', '|', '\\', '<', '>', '~', '`' };

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

        options._cursorPos = new CursorPos
        {
            Left = Console.CursorLeft,
            Top = Console.CursorTop
        };

        // get if global or local history
        JReadHistory history = options.CustomHistory ?? History;

        // Write initial input
        DrawLine(input, cursorPosition, options);

        do
        {
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
                            // First time going into history, save current input
                            originalInput = input;
                            historyIndex = history.Count - 1;
                        }
                        else if (historyIndex > 0)
                        {
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
                    if (cursorPosition > 0)
                    {
                        cursorPosition--;
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    }
                    break;
                case ConsoleKey.RightArrow:
                    if (cursorPosition < input.Length)
                    {
                        cursorPosition++;
                        Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                    }
                    break;
                case ConsoleKey.Home:
                    cursorPosition = 0;
                    Console.SetCursorPosition(options._cursorPos.Left, options._cursorPos.Top);
                    break;
                case ConsoleKey.End:
                    cursorPosition = input.Length;
                    Console.SetCursorPosition(options._cursorPos.Left + input.Length, options._cursorPos.Top);
                    break;
                case ConsoleKey.W:
                    if (IsCtrlKeyPressed(key))
                    {
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
                    if (options.EnableAutoComplete && options.AutoCompleteItems.Count > 0)
                    {
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
        CursorPos originalPos = options._cursorPos;

        Console.SetCursorPosition(originalPos.Left, originalPos.Top);

        // Calculate available space from original position to end of line
        int availableSpace = Console.WindowWidth - originalPos.Left;

        // Clear from original position to end of line
        Console.Write(new string(' ', availableSpace));

        // Go back to original position
        Console.SetCursorPosition(originalPos.Left, originalPos.Top);

        // Convert newlines to visible characters for display
        string ConvertNewlinesToVisible(string text)
        {
            return text.Replace("\n", "↵").Replace("\r", "");
        }

        // Get autocomplete suggestion if enabled
        string autoCompleteSuggestion = "";
        if (options.EnableAutoComplete && options.AutoCompleteItems.Count > 0)
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

        // Split input into parts: before cursor, and after cursor
        string beforeCursor = input.Substring(0, cursorDelPosition);
        string afterCursor = input.Substring(cursorDelPosition);

        // Convert to visible format
        string visibleBeforeCursor = ConvertNewlinesToVisible(beforeCursor);
        string visibleAfterCursor = ConvertNewlinesToVisible(afterCursor);

        // Calculate total length with suggestion
        int totalLength = visibleBeforeCursor.Length + autoCompleteSuggestion.Length + visibleAfterCursor.Length;

        string displayText;
        int displayCursorPos;

        if (totalLength > availableSpace - 3) // Need to truncate
        {
            // Show "..." and try to keep cursor area visible
            if (visibleBeforeCursor.Length > availableSpace / 2)
            {
                // Truncate from the beginning
                int keepLength = availableSpace - 3 - autoCompleteSuggestion.Length - Math.Min(visibleAfterCursor.Length, availableSpace / 4);
                if (keepLength > 0)
                {
                    int startIndex = visibleBeforeCursor.Length - keepLength;
                    string truncatedBefore = "..." + visibleBeforeCursor.Substring(startIndex);
                    displayText = truncatedBefore;
                    displayCursorPos = truncatedBefore.Length;
                }
                else
                {
                    displayText = "...";
                    displayCursorPos = 3;
                }
            }
            else
            {
                displayText = visibleBeforeCursor;
                displayCursorPos = visibleBeforeCursor.Length;
            }
        }
        else
        {
            // Everything fits
            displayText = visibleBeforeCursor;
            displayCursorPos = visibleBeforeCursor.Length;
        }

        // Write the text before cursor
        Console.Write(displayText);

        // Write autocomplete suggestion in grey right at cursor position
        if (!string.IsNullOrEmpty(autoCompleteSuggestion))
        {
            int remainingSpace = availableSpace - Console.CursorLeft + originalPos.Left;
            if (remainingSpace > 0)
            {
                string suggestionToShow = autoCompleteSuggestion.Length > remainingSpace
                    ? autoCompleteSuggestion.Substring(0, remainingSpace)
                    : autoCompleteSuggestion;

                // Store original colors
                ConsoleColor originalFg = Console.ForegroundColor;

                // Set grey color for suggestion
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(suggestionToShow);

                // Restore original color
                Console.ForegroundColor = originalFg;
            }
        }

        // Write the text after cursor (if there's space)
        int currentPos = Console.CursorLeft;
        int spaceForAfter = availableSpace - (currentPos - originalPos.Left);
        if (spaceForAfter > 0 && !string.IsNullOrEmpty(visibleAfterCursor))
        {
            string afterToShow = visibleAfterCursor.Length > spaceForAfter
                ? visibleAfterCursor.Substring(0, spaceForAfter)
                : visibleAfterCursor;
            Console.Write(afterToShow);
        }

        // Position cursor correctly
        int targetLeft = originalPos.Left + displayCursorPos;

        // Clamp targetLeft to valid range
        if (targetLeft < 0) targetLeft = 0;
        if (targetLeft >= Console.WindowWidth) targetLeft = Console.WindowWidth - 1;

        // Clamp originalPos.Top to valid range
        int targetTop = originalPos.Top;
        if (targetTop < 0) targetTop = 0;
        if (targetTop >= Console.BufferHeight) targetTop = Console.BufferHeight - 1;

        Console.SetCursorPosition(targetLeft, targetTop);
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
    /// </summary>
    private static int FindWordStart(string input, int position)
    {
        int wordStart = position - 1;
        while (wordStart >= 0 && Array.IndexOf(WordBoundaries, input[wordStart]) == -1)
        {
            wordStart--;
        }
        return wordStart + 1; // Move to the first character of the word
    }

    /// <summary>
    /// Finds the end of a word by looking forwards from the given position
    /// </summary>
    private static int FindWordEnd(string input, int position)
    {
        int wordEnd = position;
        while (wordEnd < input.Length && Array.IndexOf(WordBoundaries, input[wordEnd]) == -1)
        {
            wordEnd++;
        }
        return wordEnd;
    }

    /// <summary>
    /// Checks if the cursor is at the end of a word or at a word boundary
    /// </summary>
    private static bool IsAtEndOfWord(string input, int position)
    {
        return position >= input.Length || Array.IndexOf(WordBoundaries, input[position]) != -1;
    }
}
