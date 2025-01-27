using CSharpFunctionalExtensions;
using GitTools.Common;

namespace GitTools.Commands;

public static class TidyBranchCommand
{
    internal static async Task<Result> Run(bool debug, bool quiet, string? targetBranch)
    {
        var worker = new GitWorker(debug);

        var validRepoResult = await worker.CheckIfValidGitRepo();
        if (validRepoResult.IsFailure)
        {
            return Result.Failure("Not a git repo");
        }

        if (!quiet && !RequestConfirmation())
            return Result.Failure("User cancelled operation.");

        var branchesResult = await worker.GetBranches();
        if (branchesResult.IsFailure)
        {
            return branchesResult;
        }

        var wbresult = await worker.GetCurrentBranch();
        if (wbresult.IsFailure)
        {
            return wbresult;
        }
        Console.WriteLine($"Working branch: {wbresult.Value}");
        string? workingBranch = wbresult.Value;

        var pullResult = await worker.Pull();
        if (pullResult.IsFailure)
        {
            return pullResult;
        }
        Console.WriteLine("Pulled latest changes.");

        string? upstreamBranch = null;
        var currentUpstreamBranchResult = await worker.GetCurrentUpstreamBranch();
        if (currentUpstreamBranchResult.IsFailure)
        {
            return currentUpstreamBranchResult;
        }
        if (currentUpstreamBranchResult.Value.Count > 0)
        {
            upstreamBranch = currentUpstreamBranchResult.Value[0];
            Console.WriteLine($"Current upstream branch: {upstreamBranch}");
        }

        var remoteBranchesResult = await worker.GetRemoteBranches();

        IEnumerable<string> branches = branchesResult.Value.Concat(
            remoteBranchesResult.IsSuccess
                ? remoteBranchesResult
                    .Value.Where(_ => _ != "origin")
                    .Select(_ => _.Replace("origin/", ""))
                : Array.Empty<string>()
        );

        targetBranch ??= branches.FirstOrDefault(b => b is "main" or "master");
        if (targetBranch == null)
        {
            return Result.Failure("Target branch not found.");
        }
        Console.WriteLine($"Will use target branch: {targetBranch}");

        var backupOfCurrentBranch = $"{workingBranch}-backup";
        var checkoutResult = await worker.CheckoutNew(backupOfCurrentBranch);
        if (checkoutResult.IsFailure)
        {
            return checkoutResult;
        }
        Console.WriteLine($"Created backup branch: {backupOfCurrentBranch}");

        var checkoutTargetResult = await worker.Checkout(targetBranch);
        if (checkoutTargetResult.IsFailure)
        {
            return checkoutTargetResult;
        }

        var deleteBranchResult = await worker.DeleteBranch(workingBranch);
        if (deleteBranchResult.IsFailure)
        {
            return deleteBranchResult;
        }
        Console.WriteLine($"Deleted branch: {workingBranch}");

        var checkoutNewWorkingBranchResult = await worker.CheckoutNew(workingBranch);
        if (checkoutNewWorkingBranchResult.IsFailure)
        {
            return checkoutNewWorkingBranchResult;
        }
        Console.WriteLine($"Rereated branch {workingBranch} from {targetBranch}");

        var mergeResult = await worker.MergeAndSquash(backupOfCurrentBranch);
        if (mergeResult.IsFailure)
        {
            return mergeResult;
        }
        Console.WriteLine($"Merged {backupOfCurrentBranch} into {workingBranch}");

        var commitResult = await worker.CommitStaged($"Tidy branch {workingBranch} based on {targetBranch}");
        if (commitResult.IsFailure)
        {
            return commitResult;
        }
        Console.WriteLine($"Committed changes to {workingBranch}");

        if (upstreamBranch != null)
        {
            var setUpstreamResult = await worker.SetUpstreamBranch(upstreamBranch);
            if (setUpstreamResult.IsFailure)
            {
                return setUpstreamResult;
            }
            Console.WriteLine($"Set upstream branch to {upstreamBranch}");
            Console.WriteLine("You will likely need to force-push your changes to the remote repository.");
        }

        return Result.Success();
    }

    private static bool RequestConfirmation()
    {
        Console.WriteLine(
            "This command will create a temporary branch from target branch, squash current branch into it, delete current branch and recreate it from the temporary branch."
        );
        Console.WriteLine("Are you sure you want to continue? [y/N]");
        var key = Console.ReadKey();
        Console.WriteLine();
        return key.KeyChar is 'y' or 'Y';
    }
}
