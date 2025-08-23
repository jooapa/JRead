using System;

namespace JRead
{
    public class JReadOptions
    {
        public bool EnableDebug { get; set; } = false;
        internal CursorPos _cursorPos = new CursorPos();
    }

    public class CursorPos
    {
        public int Left { get; set; }
        public int Top { get; set; }
    }

    public static class JRead
    {
        public static string Read(string? prefillText = null, JReadOptions? options = null)
        {
#pragma warning disable CS8321
            bool IsCtrlKeyPressed(ConsoleKeyInfo keyInfo) => (keyInfo.Modifiers & ConsoleModifiers.Control) != 0;
            bool IsShiftKeyPressed(ConsoleKeyInfo keyInfo) => (keyInfo.Modifiers & ConsoleModifiers.Shift) != 0;
            bool IsAltKeyPressed(ConsoleKeyInfo keyInfo) => (keyInfo.Modifiers & ConsoleModifiers.Alt) != 0;
#pragma warning restore CS8321
            
            string input = prefillText ?? "";
            int cursorPosition = input.Length;
            ConsoleKeyInfo key;
            options ??= new JReadOptions();

            options._cursorPos = new CursorPos
            {
                Left = Console.CursorLeft,
                Top = Console.CursorTop
            };            
            // Write initial input
            Console.Write(input);

            do
            {
                key = Console.ReadKey(true);

                if (options.EnableDebug)
                {
                    Console.WriteLine($"Key pressed: {key.KeyChar}, Key: {key.Key}, Control: {key.Modifiers}, CursorPos: ({Console.CursorLeft}, {Console.CursorTop})");
                }

                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        return string.Empty;

                    case ConsoleKey.Enter:
                        Console.WriteLine();
                        return input;

                    case ConsoleKey.Backspace:
                        if (input.Length > 0 && cursorPosition > 0)
                        {
                            input = input.Remove(cursorPosition - 1, 1);
                            cursorPosition--;
                            RedrawLine(input, cursorPosition, options);
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

                    default:
                        if (!char.IsControl(key.KeyChar))
                        {
                            input = input.Insert(cursorPosition, key.KeyChar.ToString());
                            cursorPosition++;
                            RedrawLine(input, cursorPosition, options);
                        }
                        break;
                }
            } while (true);
        }

        private static void RedrawLine(string input, int cursorDelPosition, JReadOptions options)
        {
            CursorPos originalPos = options._cursorPos;

            Console.SetCursorPosition(originalPos.Left, originalPos.Top);
            
            // Calculate available space from original position to end of line
            int availableSpace = Console.WindowWidth - originalPos.Left;
            
            // Clear from original position to end of line
            Console.Write(new string(' ', availableSpace));
            
            // Go back to original position
            Console.SetCursorPosition(originalPos.Left, originalPos.Top);
            
            // If input is too long, show "..." at the start and display the end portion
            string displayText;
            int displayCursorPos;
            
            if (input.Length > availableSpace - 3) // Leave space for "..."
            {
                // Show "..." and the end portion of the input
                int endLength = availableSpace - 3; // Space for "..." prefix
                int startIndex = Math.Max(0, input.Length - endLength);
                displayText = string.Concat("...", input.AsSpan(startIndex));
                
                // Adjust cursor position relative to the displayed text
                if (cursorDelPosition >= startIndex)
                {
                    displayCursorPos = 3 + (cursorDelPosition - startIndex); // 3 for "..."
                }
                else
                {
                    displayCursorPos = 0; // Cursor is before visible area
                }
            }
            else
            {
                // Input fits, display normally
                displayText = input;
                displayCursorPos = cursorDelPosition;
            }
            
            // Write the display text
            Console.Write(displayText);
            
            // Position cursor at the correct location relative to original position
            int targetLeft = originalPos.Left + displayCursorPos;
            if (targetLeft < Console.WindowWidth)
            {
                Console.SetCursorPosition(targetLeft, originalPos.Top);
            }
            else
            {
                // Fallback: put cursor at end of line
                Console.SetCursorPosition(Console.WindowWidth - 1, originalPos.Top);
            }
        }
    }
}