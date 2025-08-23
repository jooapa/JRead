using JRead;

namespace JRead.Test;

public class Program
{
    public static void Main(string[] args)
    {
        var jr = new JRead();
        var opt = new JReadOptions
        {
            History = ["first", "second", "third"],
            EnableDebug = false,
        };
        string output = jr.ReadLine("kakka\non\nkakkaa", opt);

        Console.WriteLine($"Output: {output}");
    }
}
