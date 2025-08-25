using System;
using System.Collections.Generic;

namespace JRead;

public static class JRead
{
    // Global history instance for easy access
    public static JReadHistory History { get; } = new();

    public static string? Read(string? prefillText = null, string? beginningText = null, JReadOptions? options = null)
    {
        return ReadInternal(prefillText, options);
    }
    
    public static string? Read(string? prefillText, JReadOptions options)
    {
        return ReadInternal(prefillText, options);
    }

    private static string? ReadInternal(string? prefillText, JReadOptions? options = null)
    {

#pragma warning disable CS8321
        bool IsCtrlKeyPressed(ConsoleKeyInfo keyInfo) => (keyInfo.Modifiers & ConsoleModifiers.Control) != 0;
        bool IsShiftKeyPressed(ConsoleKeyInfo keyInfo) => (keyInfo.Modifiers & ConsoleModifiers.Shift) != 0;
        bool IsAltKeyPressed(ConsoleKeyInfo keyInfo) => (keyInfo.Modifiers & ConsoleModifiers.Alt) != 0;
#pragma warning restore CS8321
        
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
                        // Define word boundary characters
                        char[] wordBoundaries = { ' ', '"', '\'', '/', '(', ')', '[', ']', '{', '}', ',', '.', ';', ':', '!', '?', '@', '#', '$', '%', '^', '&', '*', '+', '=', '|', '\\', '<', '>', '~', '`' };

                        int wordStart = cursorPosition - 1;

                        // Find the start of the current word by looking for word boundary characters
                        while (wordStart >= 0 && Array.IndexOf(wordBoundaries, input[wordStart]) == -1)
                        {
                            wordStart--;
                        }

                        // If we found a boundary character, move one position forward to start deletion after it
                        if (wordStart >= 0 && Array.IndexOf(wordBoundaries, input[wordStart]) != -1)
                        {
                            wordStart++;
                        }
                        else
                        {
                            // If no boundary found, start from beginning
                            wordStart = 0;
                        }

                        if (wordStart < cursorPosition)
                        {
                            input = input.Remove(wordStart, cursorPosition - wordStart);
                            cursorPosition = wordStart;
                            DrawLine(input, cursorPosition, options);
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

        // If input is too long, show "..." at the start and display the end portion
        string displayText;
        int displayCursorPos;

        // Convert input to visible format for display calculations
        string visibleInput = ConvertNewlinesToVisible(input);

        if (visibleInput.Length > availableSpace - 3) // Leave space for "..."
        {
            // Show "..." and the end portion of the input
            int endLength = availableSpace - 3; // Space for "..." prefix
            int startIndex = Math.Max(0, visibleInput.Length - endLength);
            displayText = string.Concat("...", visibleInput.AsSpan(startIndex));

            // Adjust cursor position relative to the displayed text
            // Need to account for newline conversion when calculating cursor position
            string beforeCursor = input.Substring(0, Math.Min(cursorDelPosition, input.Length));
            string visibleBeforeCursor = ConvertNewlinesToVisible(beforeCursor);

            if (visibleBeforeCursor.Length >= startIndex)
            {
                displayCursorPos = 3 + (visibleBeforeCursor.Length - startIndex); // 3 for "..."
            }
            else
            {
                displayCursorPos = 0; // Cursor is before visible area
            }
        }
        else
        {
            // Input fits, display normally
            displayText = visibleInput;

            // Calculate cursor position in visible text
            string beforeCursor = input.Substring(0, Math.Min(cursorDelPosition, input.Length));
            string visibleBeforeCursor = ConvertNewlinesToVisible(beforeCursor);
            displayCursorPos = visibleBeforeCursor.Length;
        }

        // Write the display text
        Console.Write(displayText);

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
}
