namespace GitTools.Common;

public class Result
{
    public bool IsSuccess { get; protected set; }
    public string Error { get; protected set; } = string.Empty;
    public static Result Failure(string error) => new Result { IsSuccess = false, Error = error };
    public static Result Success() => new Result { IsSuccess = true };
    public static Result SuccessIf(bool condition, string error) => condition ? Success() : Failure(error);
}
