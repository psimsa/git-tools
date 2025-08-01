using System.CommandLine;
using System.CommandLine.Invocation;
using GitTools.Commands;
using GitTools.Common;

var debugOption = new Option<bool>(new[] { "--debug", "-d" }, "Show git command output");
var rootCommand = new RootCommand("Suite of small git utilities");
rootCommand.Add(debugOption);

SetupNukeCommand(rootCommand, debugOption);
SetupBootstrapCommand(rootCommand, debugOption);
SetupTidyBranchCommand(rootCommand, debugOption);

return await rootCommand.InvokeAsync(args);

static void SetupNukeCommand(RootCommand rootCommand, Option<bool> debugOption)
{
    var nukeCommand = new Command("nuke", "Clean-up current repo");
    var quietOption = new Option<bool>(new[] { "--quiet", "-q" }, "Do not ask for confirmation");
    var noSwitchBranchOption = new Option<bool>(new[] { "--no-switch-branch", "-n" }, "Do not switch to main or master branch");
    var useBranchOption = new Option<string?>(new[] { "--use-branch", "-b" }, "Branch to use instead of master/main branch");
    nukeCommand.Add(quietOption);
    nukeCommand.Add(noSwitchBranchOption);
    nukeCommand.Add(useBranchOption);

    nukeCommand.Handler = CommandHandler.Create((ParseResult parseResult) =>
        NukeCommand.Run(
            parseResult.GetValueForOption(debugOption),
            parseResult.GetValueForOption(quietOption),
            parseResult.GetValueForOption(noSwitchBranchOption),
            parseResult.GetValueForOption(useBranchOption)
        ));
    rootCommand.Add(nukeCommand);
}

static void SetupTidyBranchCommand(RootCommand rootCommand, Option<bool> debugOption)
{
    var tidyBranchCommand = new Command(
        "tidy-branch",
        "Create single squashed commit from current branch rebased onto another branch (master/main by default)"
    );
    var quietOption = new Option<bool>(new[] { "--quiet", "-q" }, "Do not ask for confirmation");
    var targetBranchOption = new Option<string?>(new[] { "--target-branch", "-t" }, "Branch to rebase onto");
    tidyBranchCommand.Add(quietOption);
    tidyBranchCommand.Add(targetBranchOption);

    tidyBranchCommand.Handler = CommandHandler.Create((ParseResult parseResult) =>
        TidyBranchCommand.Run(
            parseResult.GetValueForOption(debugOption),
            parseResult.GetValueForOption(quietOption),
            parseResult.GetValueForOption(targetBranchOption)
        ));
    rootCommand.Add(tidyBranchCommand);
}

static void SetupBootstrapCommand(RootCommand rootCommand, Option<bool> debugOption)
{
    var bootstrapCommand = new Command("bootstrap", "Bootstrap repo from parameters");
    var templateOption = new Option<string>(new[] { "--template", "-t" }, "Template to use");
    var defaultBranchOption = new Option<string>(new[] { "--default-branch", "-b" }, "Default branch to use");
    var userEmailOption = new Option<string>(new[] { "--user-email", "-e" }, "User email to use");
    bootstrapCommand.Add(templateOption);
    bootstrapCommand.Add(defaultBranchOption);
    bootstrapCommand.Add(userEmailOption);

    bootstrapCommand.Handler = CommandHandler.Create((ParseResult parseResult) =>
        BootstrapCommand.Run(
            parseResult.GetValueForOption(debugOption),
            parseResult.GetValueForOption(templateOption),
            parseResult.GetValueForOption(defaultBranchOption),
            parseResult.GetValueForOption(userEmailOption)
        ));
    rootCommand.Add(bootstrapCommand);
}
