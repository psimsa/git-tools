using CSharpFunctionalExtensions;
using GitTools.Common;

namespace GitTools.Commands;

public static class NukeCommand
{
    public static async Task Run(bool debug, bool quiet, bool noSwitchBranch, string? useBranch)
    {
        try
        {
            var worker = new GitWorker(debug);

            await worker.CheckIfValidGitRepo().ThrowOnError("Not a git repo");

            if (!quiet && !RequestConfirmation())
            {
                throw new FunctionalException("User cancelled operation.");
            }

            var localBranches = await worker.GetBranches().ThrowOnError();

            string? workingBranch;

            if (noSwitchBranch)
            {
                workingBranch = (await worker.GetCurrentBranch().ThrowOnError());
                Logger.Log($"Working branch: {workingBranch}");
            }
            else
            {
                var remoteBranchesResult = await worker.GetRemoteBranches();

                IEnumerable<string> branches = localBranches.Concat(
                    remoteBranchesResult.IsSuccess
                        ? remoteBranchesResult
                            .Value.Where(_ => _ != "origin")
                            .Select(_ => _.Replace("origin/", ""))
                        : Array.Empty<string>()
                );

                workingBranch = useBranch ?? branches.FirstOrDefault(b => b is "main" or "master");
                if (workingBranch == null)
                {
                    throw new FunctionalException("No main or master branch found.");
                }
                Logger.Log($"Will use branch: {workingBranch}");

                await worker.Reset().ThrowOnError();

                await worker.Checkout(workingBranch).ThrowOnError();
            }

            var branchesToDelete = localBranches.Where(b => b != workingBranch);
            await DeleteNonDefaultBranches(branchesToDelete, worker, workingBranch);

            await worker.Pull().ThrowOnError();
            Logger.Log("Pulled changes from remote repository.");

            await worker.Prune().ThrowOnError();
        }
        catch (FunctionalException ex)
        {
            ColorfulConsole.Log(ex.Message, ConsoleColor.Red);
        }
    }

    private static async Task DeleteNonDefaultBranches(
        IEnumerable<string> branchesToDelete,
        GitWorker gitRunner,
        string workingBranch
    )
    {
        foreach (var branch in branchesToDelete)
        {
            await gitRunner.DeleteBranch(branch).ThrowOnError();
            Logger.Log($"Deleted branch: {branch}");
        }

        Logger.Log($"All branches deleted except for {workingBranch}");
    }

    private static bool RequestConfirmation()
    {
        Logger.Log("This command will switch to main/master branch remove other local branches.");
        ColorfulConsole.Log(
            "This will undo any local changes and is not reversible.",
            ConsoleColor.Red
        );
        Logger.Log("Are you sure you want to continue? [y/N]");
        var key = Console.ReadKey();
        Logger.Log();
        return key.KeyChar is 'y' or 'Y';
    }
}
