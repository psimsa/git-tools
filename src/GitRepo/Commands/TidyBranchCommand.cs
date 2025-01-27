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

        var backupOfCurrentBranch = $"{workingBranch}-backup";
        var checkoutResult = await worker.CheckoutNew(backupOfCurrentBranch);
        if (checkoutResult.IsFailure)
        {
            return checkoutResult;
        }

        var checkoutTargetResult = await worker.Checkout(targetBranch);
        if (checkoutTargetResult.IsFailure)
        {
            return checkoutTargetResult;
        }

        pullResult = await worker.Pull();
        if (pullResult.IsFailure)
        {
            return pullResult;
        }

        var tmpBranch = $"tmp-{workingBranch}";
        var checkoutTmpResult = await worker.CheckoutNew(tmpBranch);
        if (checkoutTmpResult.IsFailure)
        {
            return checkoutTmpResult;
        }

        var mergeResult = await worker.MergeAndSquash(backupOfCurrentBranch);
        if (mergeResult.IsFailure)
        {
            return mergeResult;
        }

        var deleteResult = await worker.DeleteBranch(workingBranch);
        if (deleteResult.IsFailure)
        {
            return deleteResult;
        }

        var checkoutWorkingResult = await worker.CheckoutNew(workingBranch);
        if (checkoutWorkingResult.IsFailure)
        {
            return checkoutWorkingResult;
        }

        var commitResult = await worker.CommitStaged($"Squashed changes onto {targetBranch}");
        if (commitResult.IsFailure)
        {
            return commitResult;
        }

        var deleteTmpResult = await worker.DeleteBranch(tmpBranch);
        if (deleteTmpResult.IsFailure)
        {
            return deleteTmpResult;
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
