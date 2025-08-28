namespace JRead.Internal
{
    internal static class Construct
    {
        internal static DisplayInfo ConstructDisplayString(string input, int cursorDelPosition, JReadOptions options)
        {
            // Convert newlines to visible characters for display
            string ConvertNewlinesToVisible(string text)
            {
                return text.Replace("\n", "↵").Replace("\r", "");
            }

            // Get autocomplete suggestion if enabled
            string autoCompleteSuggestion = "";
            if (Reader._enableAutoComplete && options.AutoCompleteItems.Count > 0)
            {
                string currentWord = Functions.GetCurrentWord(input, cursorDelPosition);
                if (currentWord.Length >= options.AutoCompleteMinLength)
                {
                    // Only show suggestions if we're at the end of the current word
                    bool atEndOfWord = Functions.IsAtEndOfWord(input, cursorDelPosition);

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
            CursorPos originalPos = options._cursorPos;
            int availableWidth = Console.WindowWidth - originalPos.Left;

            // Apply MaxDisplayLength windowing if set
            int maxDisplay = options.MaxDisplayLength ?? availableWidth;
            if (options.SubtractFromAvailableSpace && options.MaxDisplayLength.HasValue)
            {
                maxDisplay = Math.Max(0, availableWidth - options.MaxDisplayLength.Value);
            }

            if (maxDisplay > 0 && maxDisplay < availableWidth)
            {
                availableWidth = maxDisplay;
            }

            if (maxDisplay > 0 && visibleInput.Length > maxDisplay)
            {
                // Center the window around the cursor
                int windowStart = Math.Max(0, Math.Min(visibleInput.Length - maxDisplay, cursorDelPosition - maxDisplay / 2));
                int windowEnd = Math.Min(visibleInput.Length, windowStart + maxDisplay);
                string windowedInput = visibleInput.Substring(windowStart, windowEnd - windowStart);
                int windowedCursor = cursorDelPosition - windowStart;
                visibleInput = windowedInput;
                cursorDelPosition = windowedCursor;
            }

            // Determine how to display based on available space
            int totalLength = visibleInput.Length + autoCompleteSuggestion.Length;

            if (totalLength <= availableWidth)
            {
                // Text fits, show everything
                string beforeCursor = visibleInput.Substring(0, Math.Min(cursorDelPosition, visibleInput.Length));
                string afterCursor = visibleInput.Substring(Math.Min(cursorDelPosition, visibleInput.Length));

                return new DisplayInfo
                {
                    BeforeCursor = beforeCursor,
                    Suggestion = autoCompleteSuggestion,
                    AfterCursor = afterCursor,
                    CursorPosition = beforeCursor.Length,
                    HasSuggestion = !string.IsNullOrEmpty(autoCompleteSuggestion)
                };
            }
            else if (availableWidth > 6)
            {
                // Need truncation
                string beforeCursor = visibleInput.Substring(0, cursorDelPosition);
                string afterCursor = visibleInput.Substring(cursorDelPosition);

                int spaceForEllipsis = 3;
                int spaceForContent = availableWidth - spaceForEllipsis;

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

                bool showStartEllipsis = cursorDelPosition > beforeCursorLength;
                bool showEndEllipsis = (cursorDelPosition + afterCursorLength) < visibleInput.Length;

                string displayBefore = "";
                int displayCursorPos = 0;

                if (showStartEllipsis)
                {
                    displayBefore += "...";
                    displayCursorPos = 3;
                }

                if (beforeCursorLength > 0)
                {
                    string textBefore = beforeCursor.Substring(Math.Max(0, beforeCursor.Length - beforeCursorLength));
                    displayBefore += textBefore;
                    displayCursorPos += textBefore.Length;
                }

                string displayAfter = "";
                if (afterCursorLength > 0)
                {
                    displayAfter = afterCursor.Substring(0, Math.Min(afterCursor.Length, afterCursorLength));
                }

                if (showEndEllipsis)
                {
                    displayAfter += "...";
                }

                return new DisplayInfo
                {
                    BeforeCursor = displayBefore,
                    Suggestion = "", // No suggestions when truncated
                    AfterCursor = displayAfter,
                    CursorPosition = displayCursorPos,
                    HasSuggestion = false
                };
            }
            else
            {
                // Terminal too narrow, show cursor position indicator
                string indicator = cursorDelPosition.ToString();
                return new DisplayInfo
                {
                    BeforeCursor = indicator.Length <= availableWidth ? indicator : "",
                    Suggestion = "",
                    AfterCursor = "",
                    CursorPosition = indicator.Length <= availableWidth ? indicator.Length : 0,
                    HasSuggestion = false
                };
            }
        }
    }
}