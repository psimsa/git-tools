using System.Diagnostics;
using System.Text;
using CSharpFunctionalExtensions;

namespace GitTools.Common;

public class GitWorker(bool isDebug)
{
    public async Task<Result<IList<string>>> GetBranches() => await ExecuteGitCommand("branch --format %(refname:short)");

    public async Task<Result<IList<string>>> GetRemoteBranches() => await ExecuteGitCommand("branch -r --format %(refname:short)");

    public async Task<Result> Checkout(string branchName) => await ExecuteGitCommand($"checkout {branchName} --force");

    public async Task<Result> Reset() => await ExecuteGitCommand("reset --hard");

    public async Task<Result> Init(string branchName) => await ExecuteGitCommand($"init && git checkout -b {branchName}");

    public async Task<Result> DeleteBranch(string branch) => await ExecuteGitCommand($"branch -D {branch}");

    public async Task<Result<string>> GetCurrentBranch() => (await ExecuteGitCommand("branch --show-current")).Map(r => r[0]);

    public async Task<Result> Prune() => await ExecuteGitCommand("prune");

    public async Task<Result> Pull() => await ExecuteGitCommand("pull --prune");

    public async Task<Result> CheckIfValidGitRepo() => await ExecuteGitCommand("rev-parse --is-inside-work-tree");

    private async Task<Result<IList<string>>> ExecuteGitCommand(string command)
    {
        ProcessStartInfo processStartInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = command,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var gitOutputLines = new List<string>();

        var sbError = new StringBuilder();

        using var process = new Process();
        process.StartInfo = processStartInfo;

        process.OutputDataReceived += (sender, e) =>
        {
            if (string.IsNullOrWhiteSpace(e.Data))
                return;
            gitOutputLines.Add(e.Data);
            if (isDebug)
                Console.WriteLine(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (string.IsNullOrWhiteSpace(e.Data))
                return;
            sbError.AppendLine(e.Data);
            if (isDebug)
                Console.WriteLine(e.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
        int code = process.ExitCode;
        process.Close();

        return Result.SuccessIf(code == 0, gitOutputLines as IList<string>, sbError.ToString());
    }
}
