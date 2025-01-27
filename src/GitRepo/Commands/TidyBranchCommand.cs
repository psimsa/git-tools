using CSharpFunctionalExtensions;
using GitTools.Common;

namespace GitTools.Commands;

public static class TidyBranchCommand
{
    internal static async Task<Result> Run(bool debug, bool quiet, string? targetBranch)
    {
        var worker = new GitWorker(debug);

        var validRepoResult = await CheckValidGitRepo(worker);
        if (validRepoResult.IsFailure)
            return Result.Failure(validRepoResult.Error);

        if (!quiet && !RequestConfirmation())
            return Result.Failure("User cancelled operation.");

        var branchesResult = await GetCombinedBranches(worker);
        if (branchesResult.IsFailure)
            return Result.Failure(branchesResult.Error);
        var allBranches = branchesResult.Value;

        var workingBranchResult = await GetWorkingBranch(worker);
        if (workingBranchResult.IsFailure)
            return Result.Failure(workingBranchResult.Error);
        var workingBranch = workingBranchResult.Value!;

        var pullResult = await PullLatestChanges(worker);
        if (pullResult.IsFailure)
            return Result.Failure(pullResult.Error);

        var upstreamBranchResult = await GetUpstreamBranch(worker);
        if (upstreamBranchResult.IsFailure)
            return Result.Failure(upstreamBranchResult.Error);
        var upstreamBranch = upstreamBranchResult.Value;

        targetBranch ??= DetermineTargetBranch(allBranches);
        if (targetBranch == null)
            return Result.Failure("Target branch not found.");
        Console.WriteLine($"Will use target branch: {targetBranch}");

        var backupBranch = $"{workingBranch}-backup";
        var backupResult = await CreateBackupBranch(worker, backupBranch);
        if (backupResult.IsFailure)
            return Result.Failure(backupResult.Error);

        var recreateResult = await RecreateWorkingBranch(worker, workingBranch, targetBranch);
        if (recreateResult.IsFailure)
            return Result.Failure(recreateResult.Error);

        var mergeResult = await MergeBackupIntoWorkingBranch(worker, backupBranch);
        if (mergeResult.IsFailure)
            return Result.Failure(mergeResult.Error);

        var commitResult = await CommitChanges(worker, workingBranch, targetBranch);
        if (commitResult.IsFailure)
            return Result.Failure(commitResult.Error);

        if (upstreamBranch != null)
        {
            var setUpstreamResult = await SetUpstreamBranch(worker, upstreamBranch);
            if (setUpstreamResult.IsFailure)
                return Result.Failure(setUpstreamResult.Error);
        }

        return Result.Success();
    }

    private static async Task<Result> CheckValidGitRepo(GitWorker worker)
    {
        var result = await worker.CheckIfValidGitRepo();
        if (result.IsFailure)
        {
            return Result.Failure("Not a git repo");
        }
        return Result.Success();
    }

    private static bool RequestConfirmation()
    {
        Console.WriteLine(
            "This command will create a temporary branch from the target branch, squash the current branch into it, delete the current branch, and recreate it from the temporary branch."
        );
        Console.WriteLine("Are you sure you want to continue? [y/N]");
        var key = Console.ReadKey();
        Console.WriteLine();
        return key.KeyChar is 'y' or 'Y';
    }

    private static async Task<Result<IEnumerable<string>>> GetCombinedBranches(GitWorker worker)
    {
        var localBranchesResult = await worker.GetBranches();
        if (localBranchesResult.IsFailure)
            return Result.Failure<IEnumerable<string>>(localBranchesResult.Error);

        var remoteBranchesResult = await worker.GetRemoteBranches();
        if (remoteBranchesResult.IsFailure)
            return Result.Failure<IEnumerable<string>>(remoteBranchesResult.Error);

        var combinedBranches = localBranchesResult.Value.Concat(
            remoteBranchesResult.Value
                .Where(branch => branch != "origin")
                .Select(branch => branch.Replace("origin/", ""))
        );

        return Result.Success(combinedBranches);
    }

    private static async Task<Result<string>> GetWorkingBranch(GitWorker worker)
    {
        var result = await worker.GetCurrentBranch();
        if (result.IsFailure)
            return Result.Failure<string>(result.Error);

        Console.WriteLine($"Working branch: {result.Value}");
        return Result.Success(result.Value);
    }

    private static async Task<Result> PullLatestChanges(GitWorker worker)
    {
        var result = await worker.Pull();
        if (result.IsFailure)
            return Result.Failure(result.Error);

        Console.WriteLine("Pulled latest changes.");
        return Result.Success();
    }

    private static async Task<Result<string?>> GetUpstreamBranch(GitWorker worker)
    {
        var result = await worker.GetCurrentUpstreamBranch();
        if (result.IsFailure)
            return Result.Failure<string?>(result.Error);

        if (result.Value.Count > 0)
        {
            var upstreamBranch = result.Value[0];
            Console.WriteLine($"Current upstream branch: {upstreamBranch}");
            return Result.Success<string?>(upstreamBranch);
        }

        return Result.Success<string?>(null);
    }

    private static string? DetermineTargetBranch(IEnumerable<string> branches)
    {
        var target = branches.FirstOrDefault(b => b is "main" or "master");
        return target;
    }

    private static async Task<Result> CreateBackupBranch(GitWorker worker, string backupBranch)
    {
        var result = await worker.CheckoutNew(backupBranch);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        Console.WriteLine($"Created backup branch: {backupBranch}");
        return Result.Success();
    }

    private static async Task<Result> RecreateWorkingBranch(GitWorker worker, string workingBranch, string targetBranch)
    {
        var checkoutTargetResult = await worker.Checkout(targetBranch);
        if (checkoutTargetResult.IsFailure)
            return Result.Failure(checkoutTargetResult.Error);

        var deleteBranchResult = await worker.DeleteBranch(workingBranch);
        if (deleteBranchResult.IsFailure)
            return Result.Failure(deleteBranchResult.Error);
        Console.WriteLine($"Deleted branch: {workingBranch}");

        var checkoutNewResult = await worker.CheckoutNew(workingBranch);
        if (checkoutNewResult.IsFailure)
            return Result.Failure(checkoutNewResult.Error);
        Console.WriteLine($"Recreated branch {workingBranch} from {targetBranch}");

        return Result.Success();
    }

    private static async Task<Result> MergeBackupIntoWorkingBranch(GitWorker worker, string backupBranch)
    {
        var result = await worker.MergeAndSquash(backupBranch);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        Console.WriteLine($"Merged {backupBranch} into the working branch");
        return Result.Success();
    }

    private static async Task<Result> CommitChanges(GitWorker worker, string workingBranch, string targetBranch)
    {
        var message = $"Tidy branch {workingBranch} based on {targetBranch}";
        var result = await worker.CommitStaged(message);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        Console.WriteLine($"Committed changes to {workingBranch}");
        return Result.Success();
    }

    private static async Task<Result> SetUpstreamBranch(GitWorker worker, string upstreamBranch)
    {
        var result = await worker.SetUpstreamBranch(upstreamBranch);
        if (result.IsFailure)
            return Result.Failure(result.Error);

        Console.WriteLine($"Set upstream branch to {upstreamBranch}");
        Console.WriteLine("You will likely need to force-push your changes to the remote repository.");
        return Result.Success();
    }
}
