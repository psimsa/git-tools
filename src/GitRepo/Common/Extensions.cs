using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace GitTools.Common;

public static class Extensions
{
    public static Result EndOnError(this Result result, string? message = null)
    {
        if (result.IsFailure)
        {
            Logger.Log(message ?? result.Error);
            Environment.Exit(1);
        }
        return result;
    }

    public static Result<T> EndOnError<T>(this Result<T> result, string? message = null)
    {
        if (result.IsFailure)
        {
            Logger.Log(message ?? result.Error);
            Environment.Exit(1);
        }
        return result;
    }

    public static async Task<Result> EndOnError(this Task<Result> task, string? message = null)
    {
        var result = await task;
        return result.EndOnError(message);
    }

    public static async Task<Result<T>> EndOnError<T>(this Task<Result<T>> task, string? message = null)
    {
        var result = await task;
        return result.EndOnError(message);
    }
}
