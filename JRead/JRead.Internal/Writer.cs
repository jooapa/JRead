namespace JRead.Internal;

internal static class Writer
{
    internal static void RenderDisplayString(DisplayInfo displayInfo, JReadOptions options)
    {
        CursorPos originalPos = options._cursorPos;
        int availableWidth = Console.WindowWidth - originalPos.Left;

        // Apply MaxDisplayLength if set
        if (options.MaxDisplayLength.HasValue)
        {
            if (options.SubtractFromAvailableSpace)
            {
                availableWidth = Math.Max(0, availableWidth - options.MaxDisplayLength.Value);
            }
            else
            {
                availableWidth = Math.Min(availableWidth, options.MaxDisplayLength.Value);
            }
        }

        // Clear the current line from the original position
        Functions.SafeSetCursorPosition(originalPos.Left, originalPos.Top);
        if (availableWidth > 0)
        {
            Console.Write(new string(' ', availableWidth));
        }

        // Return to original position
        Functions.SafeSetCursorPosition(originalPos.Left, originalPos.Top);

        // Write text before cursor
        if (!string.IsNullOrEmpty(displayInfo.BeforeCursor))
        {
            if (options.EnableMaskedInput)
                Console.Write(new string(options.MaskedInputChar, displayInfo.BeforeCursor.Length));
            else
                Console.Write(displayInfo.BeforeCursor);
        }

        // Save cursor position
        int cursorLeft = Console.CursorLeft;
        int cursorTop = Console.CursorTop;

        // Write autocomplete suggestion in gray
        if (displayInfo.HasSuggestion && !string.IsNullOrEmpty(displayInfo.Suggestion))
        {
            ConsoleColor originalFg = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(displayInfo.Suggestion);
            Console.ForegroundColor = originalFg;
        }

        // Write text after cursor
        if (!string.IsNullOrEmpty(displayInfo.AfterCursor))
        {
            if (options.EnableMaskedInput)
                Console.Write(new string(options.MaskedInputChar, displayInfo.AfterCursor.Length));
            else
                Console.Write(displayInfo.AfterCursor);
        }

        // Position cursor at the right place
        Functions.SafeSetCursorPosition(cursorLeft, cursorTop);
    }
}