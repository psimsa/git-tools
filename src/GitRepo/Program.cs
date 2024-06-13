using System.CommandLine;
using Common;
using GitBootstrap;
using GitNuke;

var rootCommand = new RootCommand("Suite of small git utilities");

var debugOption = new Option<bool>(["--debug", "-d"], () => false, "Show git command output");
rootCommand.AddGlobalOption(debugOption);

var nukeCommand = new Command("nuke", "Clean-up current repo");
var quietOption = new Option<bool>(["--quiet", "-q"], () => false, "Do not ask for confirmation");
nukeCommand.AddOption(quietOption);
var noSwitchBranchOption = new Option<bool>(["--no-switch-branch", "-n"], () => false, "Do not switch to main or master branch");
nukeCommand.AddOption(noSwitchBranchOption);
nukeCommand.SetHandler(async (debug, quiet, noSwitchBranch) =>
{
    var result = await NukeProcessor.Run(debug, quiet, noSwitchBranch);
    if (result.IsFailure)
    {
        EnhancedConsole.WriteLine(result.Error, ConsoleColor.Red);
    }
}, debugOption, quietOption, noSwitchBranchOption);
rootCommand.Add(nukeCommand);

var bootstrapCommand = new Command("bootstrap", "Bootstrap repo from parameters or template") { IsHidden = true };
bootstrapCommand.SetHandler(async (debug) =>
{
    var result = await BootstrapProcessor.Run(debug);
    if (result.IsFailure)
    {
        EnhancedConsole.WriteLine(result.Error, ConsoleColor.Red);
    }
}, debugOption);
rootCommand.Add(bootstrapCommand);

await rootCommand.InvokeAsync(args);
