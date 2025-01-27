using GitTools.Common;

namespace GitTools.Commands;

public static class TidyBranchCommand
{
    internal static async Task Run(bool debug, bool quiet, string? targetBranch)
    {
        try
        {
            var worker = new GitWorker(debug);

            await CheckValidGitRepo(worker);

            if (!quiet && !RequestConfirmation())
            {
                throw new FunctionalException("User cancelled operation.");
            }

            var allBranches = await GetCombinedBranches(worker);

            var workingBranch = await GetWorkingBranch(worker);

            await PullLatestChanges(worker);

            var upstreamBranch = await GetUpstreamBranch(worker);

            targetBranch ??= allBranches.FirstOrDefault(b => b is "main" or "master");
            if (targetBranch == null)
            {
                throw new FunctionalException("Target branch not found.");
            }
            Logger.Log($"Will use target branch: {targetBranch}");

            var backupBranch = $"{workingBranch}-backup";
            await CreateBackupBranch(worker, backupBranch);

            await RecreateWorkingBranch(worker, workingBranch, targetBranch);

            await MergeBackupIntoWorkingBranch(worker, backupBranch);

            await CommitChanges(worker, workingBranch, targetBranch);

            if (upstreamBranch != null)
            {
                await SetUpstreamBranch(worker, upstreamBranch);
            }
        }
        catch (FunctionalException e)
        {
            ColorfulConsole.Log(e.Message, ConsoleColor.Red);
        }
    }

    private static async Task CheckValidGitRepo(GitWorker worker)
    {
        await worker.CheckIfValidGitRepo().ThrowOnError("Not a git repo");
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

    private static async Task<IEnumerable<string>> GetCombinedBranches(GitWorker worker)
    {
        var localBranchesResult = await worker.GetBranches().ThrowOnError();

        var remoteBranchesResult = await worker.GetRemoteBranches().ThrowOnError();

        var combinedBranches = localBranchesResult.Concat(
            remoteBranchesResult
                .Where(branch => branch != "origin")
                .Select(branch => branch.Replace("origin/", ""))
        );

        return combinedBranches;
    }

    private static async Task<string> GetWorkingBranch(GitWorker worker)
    {
        var result = await worker.GetCurrentBranch().ThrowOnError();

        Logger.Log($"Working branch: {result}");
        return result;
    }

    private static async Task PullLatestChanges(GitWorker worker)
    {
        await worker.Pull().ThrowOnError();

        Logger.Log("Pulled latest changes.");
    }

    private static async Task<string?> GetUpstreamBranch(GitWorker worker)
    {
        var result = await worker.GetCurrentUpstreamBranch().ThrowOnError();

        if (result.Count == 0)
            return null;

        var upstreamBranch = result[0];
        Logger.Log($"Current upstream branch: {upstreamBranch}");
        return upstreamBranch;
    }

    private static async Task CreateBackupBranch(GitWorker worker, string backupBranch)
    {
        await worker.CheckoutNew(backupBranch).ThrowOnError();
        Logger.Log($"Created backup branch: {backupBranch}");
    }

    private static async Task RecreateWorkingBranch(
        GitWorker worker,
        string workingBranch,
        string targetBranch
    )
    {
        await worker.Checkout(targetBranch).ThrowOnError();

        await worker.DeleteBranch(workingBranch).ThrowOnError();
        Logger.Log($"Deleted branch: {workingBranch}");

        await worker.CheckoutNew(workingBranch).ThrowOnError();
        Logger.Log($"Recreated branch {workingBranch} from {targetBranch}");
    }

    private static async Task MergeBackupIntoWorkingBranch(
        GitWorker worker,
        string backupBranch
    )
    {
        await worker.MergeAndSquash(backupBranch).ThrowOnError();

        Logger.Log($"Merged {backupBranch} into the working branch");
    }

    private static async Task CommitChanges(
        GitWorker worker,
        string workingBranch,
        string targetBranch
    )
    {
        var message = $"Tidy branch {workingBranch} based on {targetBranch}";
        await worker.CommitStaged(message).ThrowOnError();

        Logger.Log($"Committed changes to {workingBranch}");
    }

    private static async Task SetUpstreamBranch(GitWorker worker, string upstreamBranch)
    {
        await worker.SetUpstreamBranch(upstreamBranch).ThrowOnError();

        Logger.Log($"Set upstream branch to {upstreamBranch}");
        Logger.Log("You will likely need to force-push your changes to the remote repository.");
    }
}
