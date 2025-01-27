using CSharpFunctionalExtensions;
using GitTools.Common;

namespace GitTools.Commands;

public static class BootstrapCommand
{
    public static async Task Run(
        bool debug,
        string? template,
        string defaultBranch,
        string? userEmail
    )
    {
        try
        {
            var worker = new GitWorker(debug);

            await worker.CheckIfValidGitRepo().ThrowOnError();

            await worker.Init(defaultBranch).ThrowOnError();

            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                await worker.SetConfigValue("user.email", userEmail).ThrowOnError();
            }
        }
        catch (FunctionalException e)
        {
            ColorfulConsole.Log(e.Message, ConsoleColor.Red);
        }
    }
}
