using CSharpFunctionalExtensions;
using GitTools.Common;

namespace GitTools.GitBootstrap;

public static class BootstrapProcessor
{
    public static async Task<Result> Run(bool debug, string? template, string defaultBranch, string? userEmail)
    {
        var gitWorker = new GitWorker(debug);

        return Result.Success();
    }
}
