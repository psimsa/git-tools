using CSharpFunctionalExtensions;
using GitTools.Common;

namespace GitTools.GitBootstrap;

public static class BootstrapProcessor
{
    public static async Task<Result> Run(bool debug, string? template, string defaultBranch, string? userEmail)
    {
        var worker = new GitWorker(debug);

        var validRepoResult = await worker.CheckIfValidGitRepo();
        if (validRepoResult.IsSuccess)
        {
            return Result.Failure("Already in a git repo");
        }

        var result = await worker.Init(defaultBranch);
        if (result.IsFailure)
        {
            return result;
        }

        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            result = await worker.SetConfigValue("user.email", userEmail);
            if (result.IsFailure)
            {
                return result;
            }
        }

        return result;
    }
}
