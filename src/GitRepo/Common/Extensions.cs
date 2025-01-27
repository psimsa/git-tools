using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace GitTools.Common;

public static class Extensions
{
    public static void ThrowOnError(this Result result, string? message = null)
    {
        if (result.IsFailure)
        {
            throw new FunctionalException(message ?? result.Error);
        }
    }

    public static T ThrowOnError<T>(this Result<T> result, string? message = null)
    {
        if (result.IsFailure)
        {
            throw new FunctionalException(message ?? result.Error);
        }
        return result.Value;
    }

    public static async Task ThrowOnError(this Task<Result> task, string? message = null)
    {
        var result = await task;
        result.ThrowOnError(message);
    }

    public static async Task<T> ThrowOnError<T>(this Task<Result<T>> task, string? message = null)
    {
        var result = await task;
        return result.ThrowOnError(message);
    }
}

internal class FunctionalException : Exception
{
    public FunctionalException(string message)
        : base(message) { }

    public FunctionalException()
        : base() { }

    public FunctionalException(string? message, Exception? innerException)
        : base(message, innerException) { }
}
