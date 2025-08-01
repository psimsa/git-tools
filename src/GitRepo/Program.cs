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
    nukeCommand.Add(debugOption);

    var quietOption = new Option<bool>("--quiet", ["-q"]);
    quietOption.Description = "Do not ask for confirmation";
    nukeCommand.Add(quietOption);

    var noSwitchBranchOption = new Option<bool>("--no-switch-branch", ["-n"]);
    noSwitchBranchOption.Description = "Do not switch to main or master branch";
    nukeCommand.Add(noSwitchBranchOption);

    var useBranchOption = new Option<string?>("--use-branch", ["-b"]);
    useBranchOption.Description = "Branch to use instead of master/main branch";
    nukeCommand.Add(useBranchOption);

    nukeCommand.SetAction(async (ParseResult parseResult) =>
        await NukeCommand.Run(
            parseResult.GetValue(debugOption),
            parseResult.GetValue(quietOption),
            parseResult.GetValue(noSwitchBranchOption),
            parseResult.GetValue(useBranchOption)
        ));
    rootCommand.Add(nukeCommand);
}

static void SetupTidyBranchCommand(RootCommand rootCommand, Option<bool> debugOption)
{
    var tidyBranchCommand = new Command(
        "tidy-branch",
        "Create single squashed commit from current branch rebased onto another branch (master/main by default)"
    );
    tidyBranchCommand.Add(debugOption);

    var quietOption = new Option<bool>("--quiet", ["-q"]);
    quietOption.Description = "Do not ask for confirmation";
    tidyBranchCommand.Add(quietOption);

    var targetBranchOption = new Option<string?>("--target-branch", ["-t"]);
    targetBranchOption.Description = "Branch to rebase onto";
    tidyBranchCommand.Add(targetBranchOption);

    tidyBranchCommand.SetAction(async (ParseResult parseResult) =>
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
    bootstrapCommand.Add(debugOption);

    var templateOption = new Option<string>("--template", ["-t"]);
    templateOption.Description = "Template to use";
    templateOption.Hidden = true; // Hide this option from the help output because it's not implemented yet
    bootstrapCommand.Add(templateOption);

    var defaultBranchOption = new Option<string>("--default-branch", ["-b"]);
    defaultBranchOption.Description = "Default branch to use";
    bootstrapCommand.Add(defaultBranchOption);

    var userEmailOption = new Option<string>("--user-email", ["-e"]);
    userEmailOption.Description = "User email to use";
    bootstrapCommand.Add(userEmailOption);

    bootstrapCommand.SetAction(async (ParseResult parseResult) =>
        await BootstrapCommand.Run(
            parseResult.GetValue(debugOption),
            parseResult.GetValue(templateOption),
            parseResult.GetValue(defaultBranchOption) ?? "main",
            parseResult.GetValue(userEmailOption)
        ));
    rootCommand.Add(bootstrapCommand);
}
