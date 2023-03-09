namespace NumeralSynthesizer;

public static class ConsoleHelper
{
    public static void WriteLineWithColor(ConsoleColor color, string text)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void WithColor(ConsoleColor color, Action action)
    {
        Console.ForegroundColor = color;
        action();
        Console.ForegroundColor = ConsoleColor.White;
    }
}