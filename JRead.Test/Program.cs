using JRead;

namespace JRead.Test;

public class Program
{
    public static void Main(string[] args)
    {
        var opt = new JReadOptions
        {
            EnableDebug = false,
        };
        Console.SetCursorPosition(10, Console.GetCursorPosition().Top);
        string output = JRead.Read("first\nsecond", opt);

        Console.WriteLine($"\nOutput: {output}");
    }
}
