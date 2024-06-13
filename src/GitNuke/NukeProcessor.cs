using CSharpFunctionalExtensions;
using Spectre.Console;

namespace GitNuke;

public class NukeProcessor(bool printGitOutput, bool requestConfirmation, bool noSwitchBranch) : Common.IProcessor
{
    private static bool RequestConfirmation()
    {
        AnsiConsole.MarkupLine("This command will switch to main/master branch remove other local branches.");
        AnsiConsole.MarkupLine("[underline red]This will undo any local changes and is not reversible.[/]");
        AnsiConsole.MarkupLine("Are you sure you want to continue? [[y/N]]");
        var key = Console.ReadKey();
        AnsiConsole.MarkupLine("");
        return key.KeyChar is 'y' or 'Y';
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
            AnsiConsole.MarkupLineInterpolated($"Deleted branch: {branch}");
        }

        AnsiConsole.MarkupLineInterpolated($"All branches deleted except for {workingBranch}");
        return Result.Success();
    }

    public async Task<Result> Run()
    {
        if (requestConfirmation && !RequestConfirmation()) return Result.Failure("User cancelled operation.");

        var runner = new GitWorker(printGitOutput);

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
            AnsiConsole.MarkupLineInterpolated($"Working branch: {wbresult.Value}");
            workingBranch = wbresult.Value;
        }
        else
        {
            var remoteBranchesResult = await runner.GetRemoteBranches();

            IEnumerable<string> branches = branchesResult.Value.Concat(remoteBranchesResult.IsSuccess ? remoteBranchesResult.Value.Where(_ => _ != "origin").Select(_ => _.Replace("origin/", "")) : Array.Empty<string>());

            workingBranch = branches.FirstOrDefault(b => b is "main" or "master");
            if (workingBranch == null)
            {
                return Result.Failure("No main or master branch found.");
            }
            AnsiConsole.MarkupLineInterpolated($"Found main or master branch: {workingBranch}");

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
        AnsiConsole.MarkupLine("Pulled changes from remote repository.");

        result = await runner.Prune();
        if (result.IsFailure)
        {
            return result;
        }

        return Result.Success();
    }
}
