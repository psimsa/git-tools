using CSharpFunctionalExtensions;
using GitTools.Common;

namespace GitTools.Commands;

public static class NukeCommand
{
    public static async Task<Result> Run(
        bool debug,
        bool quiet,
        bool noSwitchBranch,
        string? useBranch
    )
    {
        var worker = new GitWorker(debug);

        await worker.CheckIfValidGitRepo().EndOnError("Not a git repo");

        if (!quiet && !RequestConfirmation())
            return Result.Failure("User cancelled operation.");

        var branchesResult = await worker.GetBranches().EndOnError();

        string? workingBranch;

        if (noSwitchBranch)
        {
            workingBranch = (await worker.GetCurrentBranch().EndOnError()).Value;
            Logger.Log($"Working branch: {workingBranch}");
        }
        else
        {
            var remoteBranchesResult = await worker.GetRemoteBranches();

            IEnumerable<string> branches = branchesResult.Value.Concat(
                remoteBranchesResult.IsSuccess
                    ? remoteBranchesResult
                        .Value.Where(_ => _ != "origin")
                        .Select(_ => _.Replace("origin/", ""))
                    : Array.Empty<string>()
            );

            workingBranch = useBranch ?? branches.FirstOrDefault(b => b is "main" or "master");
            if (workingBranch == null)
            {
                return Result.Failure("No main or master branch found.");
            }
            Logger.Log($"Will use branch: {workingBranch}");

            await worker.Reset().EndOnError();

            await worker.Checkout(workingBranch).EndOnError();
        }

        var branchesToDelete = branchesResult.Value.Where(b => b != workingBranch);
        await DeleteNonDefaultBranches(branchesToDelete, worker, workingBranch).EndOnError();

        await worker.Pull().EndOnError();
        Logger.Log("Pulled changes from remote repository.");

        await worker.Prune().EndOnError();

        return Result.Success();
    }

    private static async Task<Result> DeleteNonDefaultBranches(
        IEnumerable<string> branchesToDelete,
        GitWorker gitRunner,
        string workingBranch
    )
    {
        foreach (var branch in branchesToDelete)
        {
            await gitRunner.DeleteBranch(branch).EndOnError();
            Logger.Log($"Deleted branch: {branch}");
        }

        Logger.Log($"All branches deleted except for {workingBranch}");
        return Result.Success();
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
