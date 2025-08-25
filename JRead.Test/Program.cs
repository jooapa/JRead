using JRead;
using System.Reflection;

namespace JRead.Test;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Available tests:");
            ListAvailableTests();
            Console.WriteLine("\nUsage: dotnet run <test_name>");
            return;
        }

        string testName = args[0];
        RunTest(testName);
    }

    private static void RunTest(string testName)
    {
        var methods = typeof(JReadTests).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .ToList();

        var testMethod = methods.FirstOrDefault(m =>
            string.Equals(m.Name, testName, StringComparison.OrdinalIgnoreCase));

        if (testMethod == null)
        {
            Console.WriteLine($"Test '{testName}' not found!");
            Console.WriteLine("\nAvailable tests:");
            ListAvailableTests();
            return;
        }

        Console.WriteLine($"Running test: {testMethod.Name}");
        Console.WriteLine(new string('=', 50));

        // try
        {
            testMethod.Invoke(null, null);
        }
        // catch (Exception ex)
        // {
        //     Console.WriteLine($"Test failed with error: {ex.Message}");
        // }
    }

    private static void ListAvailableTests()
    {
        var methods = typeof(JReadTests).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                   .ToList();

        foreach (var method in methods)
        {
            Console.WriteLine($"  - {method.Name}");
        }
    }
}