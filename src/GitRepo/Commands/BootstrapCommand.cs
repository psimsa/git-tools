using CSharpFunctionalExtensions;
using GitTools.Common;

namespace GitTools.Commands;

public static class BootstrapCommand
{
    public static async Task<Result> Run(
        bool debug,
        string? template,
        string defaultBranch,
        string? userEmail
    )
    {
        var worker = new GitWorker(debug);

        await worker.CheckIfValidGitRepo().EndOnError();

        var result = await worker.Init(defaultBranch).EndOnError();

        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            result = await worker.SetConfigValue("user.email", userEmail).EndOnError();
        }

        return result;
    }
}
