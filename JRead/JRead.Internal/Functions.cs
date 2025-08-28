namespace JRead.Internal;

internal static class Functions
{
    internal static string GetCurrentWord(string input, int cursorPosition)
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
    internal static int FindWordStart(string input, int position)
    {
        if (position <= 0) return 0;

        // Start from the character before the cursor
        int current = position - 1;

        // If we're starting on a word boundary (like space), skip backwards through word boundaries
        if (current >= 0 && Array.IndexOf(Reader.AutocompleteWordBoundaries, input[current]) != -1)
        {
            // Skip backwards through word boundaries (spaces, punctuation, etc.)
            while (current >= 0 && Array.IndexOf(Reader.AutocompleteWordBoundaries, input[current]) != -1)
            {
                current--;
            }
        }

        // Now skip backwards through the word characters to find the start
        while (current >= 0 && Array.IndexOf(Reader.AutocompleteWordBoundaries, input[current]) == -1)
        {
            current--;
        }

        return current + 1; // Move to the first character of the word
    }

    /// <summary>
    /// Finds the end of a word by looking forwards from the given position
    /// Handles spaces intelligently for word navigation
    /// </summary>
    internal static int FindWordEnd(string input, int position)
    {
        if (position >= input.Length) return input.Length;

        int current = position;

        // If we're starting on a word boundary (like space), skip forward through word boundaries
        if (current < input.Length && Array.IndexOf(Reader.AutocompleteWordBoundaries, input[current]) != -1)
        {
            // Skip forward through word boundaries (spaces, punctuation, etc.)
            while (current < input.Length && Array.IndexOf(Reader.AutocompleteWordBoundaries, input[current]) != -1)
            {
                current++;
            }
        }

        // Now skip forward through the word characters to find the end
        while (current < input.Length && Array.IndexOf(Reader.AutocompleteWordBoundaries, input[current]) == -1)
        {
            current++;
        }

        return current;
    }

    /// <summary>
    /// Checks if the cursor is at the end of a word or at a word boundary
    /// </summary>
    internal static bool IsAtEndOfWord(string input, int position)
    {
        return position >= input.Length || Array.IndexOf(Reader.AutocompleteWordBoundaries, input[position]) != -1;
    }

    internal static void SafeSetCursorPosition(int left, int top)
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
}