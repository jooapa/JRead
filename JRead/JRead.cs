using JRead.Internal;

namespace JRead;

public static class JRead
{
    public static JReadHistory History { get; } = new();

    /// <summary>
    /// Reads a line, but will not return null.
    /// </summary>
    /// <param name="preText"></param>
    /// <param name="startText"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string Read(string? preText = null, string startText = "", JReadOptions? options = null)
    {
        return Reader.ReadInternal(preText, options, startText) ?? string.Empty;
    }

    /// <summary>
    /// Reads a line, but will not return null.
    /// </summary>
    /// <param name="preText"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string Read(string? preText, JReadOptions options)
    {
        return Reader.ReadInternal(preText, options, "") ?? string.Empty;
    }

    /// <summary>
    /// Reads a line, but if EscapingReturnsTheOriginalInput is false, and escaping. will function return null. 
    /// </summary>
    /// <param name="preText"></param>
    /// <param name="startText"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string? ReadNull(string? preText = null, string startText = "", JReadOptions? options = null)
    {
        return Reader.ReadInternal(preText, options, startText);
    }

    /// <summary>
    /// Reads a line, but if EscapingReturnsTheOriginalInput is false, and escaping. Function will return null. 
    /// </summary>
    /// <param name="preText"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static string? ReadNull(string? preText, JReadOptions options)
    {
        return Reader.ReadInternal(preText, options, "");
    }

    /// <summary>
    /// Clears the key buffer.
    /// </summary>
    public static void ClearKeyBuffer()
    {
        while (Console.KeyAvailable)
        {
            Console.ReadKey(true);
        }
    }
}
