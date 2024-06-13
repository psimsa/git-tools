using System.CommandLine;
using GitNuke;

namespace GitRepo;

internal static class Program
{
    static async Task Main(string[] args)
    {
        var rootCommand = new RootCommand("Suite of small git utilities");

        var debugOption = new Option<bool>(["--debug", "-d"], () => false, "Show git command output");
        rootCommand.AddGlobalOption(debugOption);

        var nukeCommand = new Command("nuke", "Clean-up current repo");
        var quietOption = new Option<bool>(["--quiet", "-q"], () => false, "Do not ask for confirmation");
        nukeCommand.AddOption(quietOption);
        var noSwitchBranchOption = new Option<bool>(["--no-switch-branch", "-n"], () => false, "Do not switch to main or master branch");
        nukeCommand.AddOption(noSwitchBranchOption);
        nukeCommand.SetHandler(async (bool debug, bool quiet, bool noSwitchBranch) =>
        {
            var p = new NukeProcessor(debug, !quiet, noSwitchBranch);
            var result = await p.Run();

            if (result.IsFailure)
            {
                Console.WriteLine($"Error: {result.Error}");
                Environment.Exit(-1);
            }

            Console.WriteLine("Git repo nuked.");
        }, debugOption, quietOption, noSwitchBranchOption);
        rootCommand.Add(nukeCommand);

        var bootstrapCommand = new Command("bootstrap", "Bootstrap repo from parameters or template") { IsHidden = true };
        rootCommand.Add(bootstrapCommand);

        await rootCommand.InvokeAsync(args);
    }
}