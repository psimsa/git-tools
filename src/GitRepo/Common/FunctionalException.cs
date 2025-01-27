namespace GitTools.Common;

public class FunctionalException : Exception
{
    public FunctionalException(string message)
        : base(message) { }

    public FunctionalException()
        : base() { }

    public FunctionalException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
