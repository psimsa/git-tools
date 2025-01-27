using CSharpFunctionalExtensions;
using GitTools.Common;

namespace GitTools.Commands;

public static class TidyBranchCommand
{
    internal static async Task<Result> Run(bool debug, bool quiet, string? targetBranch)
    {
        var worker = new GitWorker(debug);

        await CheckValidGitRepo(worker).EndOnError();

        if (!quiet && !RequestConfirmation())
            return Result.Failure("User cancelled operation.");

        var allBranches = (await GetCombinedBranches(worker).EndOnError()).Value;

        var workingBranch = (await GetWorkingBranch(worker).EndOnError()).Value!;

        await PullLatestChanges(worker).EndOnError();

        var upstreamBranch = (await GetUpstreamBranch(worker).EndOnError()).Value;

        targetBranch ??= allBranches.FirstOrDefault(b => b is "main" or "master");
        if (targetBranch == null)
            return Result.Failure("Target branch not found.");
        Logger.Log($"Will use target branch: {targetBranch}");

        var backupBranch = $"{workingBranch}-backup";
        await CreateBackupBranch(worker, backupBranch).EndOnError();

        await RecreateWorkingBranch(worker, workingBranch, targetBranch).EndOnError();

        await MergeBackupIntoWorkingBranch(worker, backupBranch).EndOnError();

        await CommitChanges(worker, workingBranch, targetBranch).EndOnError();

        if (upstreamBranch != null)
        {
            await SetUpstreamBranch(worker, upstreamBranch).EndOnError();
        }

        return Result.Success();
    }

    private static async Task<Result> CheckValidGitRepo(GitWorker worker)
    {
        await worker.CheckIfValidGitRepo().EndOnError("Not a git repo");
        return Result.Success();
    }

    private static bool RequestConfirmation()
    {
        Logger.Log(
            "This command will create a temporary branch from the target branch, squash the current branch into it, delete the current branch, and recreate it from the temporary branch."
        );
        Logger.Log("Are you sure you want to continue? [y/N]");
        var key = Console.ReadKey();
        Logger.Log();
        return key.KeyChar is 'y' or 'Y';
    }

    private static async Task<Result<IEnumerable<string>>> GetCombinedBranches(GitWorker worker)
    {
        var localBranchesResult = await worker.GetBranches().EndOnError();

        var remoteBranchesResult = await worker.GetRemoteBranches().EndOnError();

        var combinedBranches = localBranchesResult.Value.Concat(
            remoteBranchesResult
                .Value.Where(branch => branch != "origin")
                .Select(branch => branch.Replace("origin/", ""))
        );

        return Result.Success(combinedBranches);
    }

    private static async Task<Result<string>> GetWorkingBranch(GitWorker worker)
    {
        var result = await worker.GetCurrentBranch().EndOnError();

        Logger.Log($"Working branch: {result.Value}");
        return Result.Success(result.Value);
    }

    private static async Task<Result> PullLatestChanges(GitWorker worker)
    {
        await worker.Pull().EndOnError();

        Logger.Log("Pulled latest changes.");
        return Result.Success();
    }

    private static async Task<Result<string?>> GetUpstreamBranch(GitWorker worker)
    {
        var result = await worker.GetCurrentUpstreamBranch().EndOnError();

        if (result.Value.Count > 0)
        {
            var upstreamBranch = result.Value[0];
            Logger.Log($"Current upstream branch: {upstreamBranch}");
            return Result.Success<string?>(upstreamBranch);
        }

        return Result.Success<string?>(null);
    }

    private static async Task<Result> CreateBackupBranch(GitWorker worker, string backupBranch)
    {
        await worker.CheckoutNew(backupBranch).EndOnError();
        Logger.Log($"Created backup branch: {backupBranch}");
        return Result.Success();
    }

    private static async Task<Result> RecreateWorkingBranch(
        GitWorker worker,
        string workingBranch,
        string targetBranch
    )
    {
        await worker.Checkout(targetBranch).EndOnError();

        await worker.DeleteBranch(workingBranch).EndOnError();
        Logger.Log($"Deleted branch: {workingBranch}");

        await worker.CheckoutNew(workingBranch).EndOnError();
        Logger.Log($"Recreated branch {workingBranch} from {targetBranch}");

        return Result.Success();
    }

    private static async Task<Result> MergeBackupIntoWorkingBranch(
        GitWorker worker,
        string backupBranch
    )
    {
        await worker.MergeAndSquash(backupBranch).EndOnError();

        Logger.Log($"Merged {backupBranch} into the working branch");
        return Result.Success();
    }

    private static async Task<Result> CommitChanges(
        GitWorker worker,
        string workingBranch,
        string targetBranch
    )
    {
        var message = $"Tidy branch {workingBranch} based on {targetBranch}";
        await worker.CommitStaged(message).EndOnError();

        Logger.Log($"Committed changes to {workingBranch}");
        return Result.Success();
    }

    private static async Task<Result> SetUpstreamBranch(GitWorker worker, string upstreamBranch)
    {
        await worker.SetUpstreamBranch(upstreamBranch).EndOnError();

        Logger.Log($"Set upstream branch to {upstreamBranch}");
        Logger.Log("You will likely need to force-push your changes to the remote repository.");
        return Result.Success();
    }
}
