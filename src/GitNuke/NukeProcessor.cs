using Common;
using CSharpFunctionalExtensions;

namespace GitNuke;

public static class NukeProcessor
{
    public static async Task<Result> Run(bool debug, bool quiet, bool noSwitchBranch)
    {
        var runner = new GitWorker(debug);

        var validRepoResult = await runner.CheckIfValidGitRepo();
        if (validRepoResult.IsFailure)
        {
            return Result.Failure("Not a git repo");
        }

        if (!quiet && !RequestConfirmation()) return Result.Failure("User cancelled operation.");

        var branchesResult = await runner.GetBranches();
        if (branchesResult.IsFailure)
        {
            return branchesResult;
        }

        string? workingBranch;
        Result result;

        if (noSwitchBranch)
        {
            var wbresult = await runner.GetCurrentBranch();
            if (wbresult.IsFailure)
            {
                return wbresult;
            }
            Console.WriteLine($"Working branch: {wbresult.Value}");
            workingBranch = wbresult.Value;
        }
        else
        {
            var remoteBranchesResult = await runner.GetRemoteBranches();

            IEnumerable<string> branches = branchesResult.Value.Concat(remoteBranchesResult.IsSuccess
                ? remoteBranchesResult.Value.Where(_ => _ != "origin").Select(_ => _.Replace("origin/", ""))
                : Array.Empty<string>());

            workingBranch = branches.FirstOrDefault(b => b is "main" or "master");
            if (workingBranch == null)
            {
                return Result.Failure("No main or master branch found.");
            }
            Console.WriteLine($"Found main or master branch: {workingBranch}");

            result = await runner.Reset();
            if (result.IsFailure)
            {
                return result;
            }

            result = await runner.Checkout(workingBranch);
            if (result.IsFailure)
            {
                return result;
            }
        }

        var branchesToDelete = branchesResult.Value.Where(b => b != workingBranch);
        result = await DeleteNonDefaultBranches(branchesToDelete, runner, workingBranch);
        if (result.IsFailure)
        {
            return result;
        }

        result = await runner.Pull();
        if (result.IsFailure)
        {
            return result;
        }
        Console.WriteLine("Pulled changes from remote repository.");

        result = await runner.Prune();
        if (result.IsFailure)
        {
            return result;
        }

        return Result.Success();
    }

    private static async Task<Result> DeleteNonDefaultBranches(IEnumerable<string> branchesToDelete, GitWorker gitRunner, string workingBranch)
    {
        foreach (var branch in branchesToDelete)
        {
            var deleteResult = await gitRunner.DeleteBranch(branch);
            if (deleteResult.IsFailure)
            {
                return deleteResult;
            }
            Console.WriteLine($"Deleted branch: {branch}");
        }

        Console.WriteLine($"All branches deleted except for {workingBranch}");
        return Result.Success();
    }

    private static bool RequestConfirmation()
    {
        Console.WriteLine("This command will switch to main/master branch remove other local branches.");
        EnhancedConsole.WriteLine("This will undo any local changes and is not reversible.", ConsoleColor.Red);
        Console.WriteLine("Are you sure you want to continue? [y/N]");
        var key = Console.ReadKey();
        Console.WriteLine();
        return key.KeyChar is 'y' or 'Y';
    }
}
