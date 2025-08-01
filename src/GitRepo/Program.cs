using System.CommandLine;
using GitTools.Commands;
using GitTools.Common;

var debugOption = new Option<bool>("--debug");
debugOption.Aliases.Add("-d");
debugOption.Description = "Show git command output";
var rootCommand = new RootCommand("Suite of small git utilities");
rootCommand.Add(debugOption);

SetupNukeCommand(rootCommand, debugOption);
SetupBootstrapCommand(rootCommand, debugOption);
SetupTidyBranchCommand(rootCommand, debugOption);

var result = rootCommand.Parse(args);
return await result.InvokeAsync();

static void SetupNukeCommand(RootCommand rootCommand, Option<bool> debugOption)
{
    var nukeCommand = new Command("nuke", "Clean-up current repo");
    var quietOption = new Option<bool>("--quiet");
    quietOption.Aliases.Add("-q");
    quietOption.Description = "Do not ask for confirmation";
    var noSwitchBranchOption = new Option<bool>("--no-switch-branch");
    noSwitchBranchOption.Aliases.Add("-n");
    noSwitchBranchOption.Description = "Do not switch to main or master branch";
    var useBranchOption = new Option<string?>("--use-branch");
    useBranchOption.Aliases.Add("-b");
    useBranchOption.Description = "Branch to use instead of master/main branch";
    nukeCommand.Add(debugOption);
    nukeCommand.Add(quietOption);
    nukeCommand.Add(noSwitchBranchOption);
    nukeCommand.Add(useBranchOption);

    nukeCommand.SetHandler(
        async (bool debug, bool quiet, bool noSwitchBranch, string? useBranch) =>
            await NukeCommand.Run(debug, quiet, noSwitchBranch, useBranch),
        debugOption, quietOption, noSwitchBranchOption, useBranchOption
    );
    rootCommand.Add(nukeCommand);
}

static void SetupTidyBranchCommand(RootCommand rootCommand, Option<bool> debugOption)
{
    var tidyBranchCommand = new Command(
        "tidy-branch",
        "Create single squashed commit from current branch rebased onto another branch (master/main by default)"
    );
    var quietOption = new Option<bool>("--quiet");
    quietOption.Aliases.Add("-q");
    quietOption.Description = "Do not ask for confirmation";
    var targetBranchOption = new Option<string?>("--target-branch");
    targetBranchOption.Aliases.Add("-t");
    targetBranchOption.Description = "Branch to rebase onto";
    tidyBranchCommand.Add(debugOption);
    tidyBranchCommand.Add(quietOption);
    tidyBranchCommand.Add(targetBranchOption);

    tidyBranchCommand.SetAction(async (System.CommandLine.ParseResult parseResult) =>
        await TidyBranchCommand.Run(
            parseResult.GetValue(debugOption),
            parseResult.GetValue(quietOption),
            parseResult.GetValue(targetBranchOption)
        ));
    rootCommand.Add(tidyBranchCommand);
}

static void SetupBootstrapCommand(RootCommand rootCommand, Option<bool> debugOption)
{
    var bootstrapCommand = new Command("bootstrap", "Bootstrap repo from parameters");
    var templateOption = new Option<string>("--template");
    templateOption.Aliases.Add("-t");
    templateOption.Description = "Template to use";
    var defaultBranchOption = new Option<string>("--default-branch");
    defaultBranchOption.Aliases.Add("-b");
    defaultBranchOption.Description = "Default branch to use";
    var userEmailOption = new Option<string>("--user-email");
    userEmailOption.Aliases.Add("-e");
    userEmailOption.Description = "User email to use";
    bootstrapCommand.Add(debugOption);
    bootstrapCommand.Add(templateOption);
    bootstrapCommand.Add(defaultBranchOption);
    bootstrapCommand.Add(userEmailOption);

    bootstrapCommand.SetAction(async (System.CommandLine.ParseResult parseResult) =>
        await BootstrapCommand.Run(
            parseResult.GetValue(debugOption),
            parseResult.GetValue(templateOption),
            parseResult.GetValue(defaultBranchOption) ?? "main",
            parseResult.GetValue(userEmailOption)
        ));
    rootCommand.Add(bootstrapCommand);
}
