namespace GitTools.Common;

internal static class Logger
{
    public static void Log() => Console.WriteLine();

    public static void Log(string message) => Logger.Log(message);
}
