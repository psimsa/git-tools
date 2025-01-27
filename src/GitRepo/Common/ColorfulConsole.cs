namespace GitTools.Common;

public static class ColorfulConsole
{
    public static void Log(string message, ConsoleColor color)
    {
        var currentColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Logger.Log(message);
        Console.ForegroundColor = currentColor;
    }
}
