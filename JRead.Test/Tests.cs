using JRead;
using System.Reflection;

namespace JRead.Test;

public class JReadTests
{
    public static void t()
    {
        Console.WriteLine("Basic JRead test:");
        string? output = JRead.Read("Enter something: ");
        Console.WriteLine($"You entered: '{output}'");
    }

    public static void h()
    {
        var opt = new JReadOptions
        {
            EnableDebug = true,
            AddReturnedValueToHistory = true,
            EscapingReturnsTheOriginalInput = true,
            CustomHistory = new(
                [
                    "Custom history item 1",
                    "Custom history item 2"
                ]
            ),
        };

        Console.SetCursorPosition(10, Console.GetCursorPosition().Top);
        string? output = JRead.Read("kakka ", "", opt);
        Console.WriteLine($"\nOutput: {output}");
        Console.WriteLine($"Global History: [{string.Join(", ", JRead.History.GetAll())}]");
        Console.WriteLine($"Custom History: [{string.Join(", ", opt.CustomHistory.GetAll())}]");
    }

    public static void a()
    {
        var options = new JReadOptions
        {
            EnableAutoComplete = true,
            AutoCompleteMinLength = 1,
            AutoCompleteCaseSensitive = false,
            AutoCompleteItems = ["cat", "clear", "copy", "create", "check", "Cat", "Clear", "help", "history", "list", "move", "remove"]
        };
        
        while (true)
        {
            string result = JRead.ReadNoNull("","Auto Complete> ", options);
            Console.WriteLine($"You entered: {result}");
        }
    }

    public static void ac()
    {        
        var options = new JReadOptions
        {
            EnableAutoComplete = true,
            AutoCompleteMinLength = 1,
            AutoCompleteCaseSensitive = true,
            AutoCompleteItems = new List<string> { "cat", "clear", "copy", "create", "check", "Cat", "Clear", "help", "history", "list", "move", "remove" }
        };
        
        while (true)
        {
            string result = JRead.ReadNoNull("","CaseSensitive> ", options);
            Console.WriteLine($"You entered: {result}");
        }
    }
}