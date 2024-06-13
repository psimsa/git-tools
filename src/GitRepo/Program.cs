using System.CommandLine;
using GitTools.Common;
using GitTools.GitBootstrap;
using GitTools.GitNuke;

var rootCommand = new RootCommand("Suite of small git utilities");

var debugOption = new Option<bool>(["--debug", "-d"], () => false, "Show git command output");
rootCommand.AddGlobalOption(debugOption);

SetupNukeCommand(rootCommand, debugOption);

SetupBootstrapCommand(rootCommand, debugOption);

await rootCommand.InvokeAsync(args);

static void SetupNukeCommand(RootCommand rootCommand, Option<bool> debugOption)
{
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
}

static void SetupBootstrapCommand(RootCommand rootCommand, Option<bool> debugOption)
{
    var bootstrapCommand = new Command("bootstrap", "Bootstrap repo from parameters or template");

    var templateOption = new Option<string>(new[] { "--template", "-t" }, "Template to use") { IsHidden = true };
    bootstrapCommand.AddOption(templateOption);
    var defaultBranchOption = new Option<string>(new[] { "--default-branch", "-b" }, () => "main", "Default branch to use");
    bootstrapCommand.AddOption(defaultBranchOption);
    var userEmailOption = new Option<string>(new[] { "--user-email", "-e" }, "User email to use");
    bootstrapCommand.AddOption(userEmailOption);

    bootstrapCommand.SetHandler(async (debug, template, defaultBranch, userEmail) =>
    {
        var result = await BootstrapProcessor.Run(debug, template, defaultBranch, userEmail);
        if (result.IsFailure)
        {
            EnhancedConsole.WriteLine(result.Error, ConsoleColor.Red);
        }
    }, debugOption, templateOption, defaultBranchOption, userEmailOption);
    rootCommand.Add(bootstrapCommand);
}