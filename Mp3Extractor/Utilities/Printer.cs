namespace Mp3Extractor.Utilities;

internal static class Printer
{
    internal static void Print(string text, ConsoleColor? color = null)
    {
        if (color.HasValue)
            Console.ForegroundColor = color.Value;

        Console.Write(text);

        if (color.HasValue)
            Console.ResetColor();
    }

    internal static void PrintLine(string text, ConsoleColor? color = null)
    {
        if (color.HasValue)
            Console.ForegroundColor = color.Value;

        Console.WriteLine(text);

        if (color.HasValue)
            Console.ResetColor();
    }
}
