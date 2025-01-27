namespace GitTools.Common;

public class Result<T> : Result
{
    public T Value { get; protected set; } = default!;
    public static new Result<T> Failure(string error) => new Result<T> { IsSuccess = false, Error = error };
    public static Result<T> Success(T value) => new Result<T> { IsSuccess = true, Value = value };
    public static Result<T> SuccessIf(bool condition, T value, string error) => condition ? Success(value) : Failure(error);
}