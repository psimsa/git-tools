using System.CommandLine;
using GitTools.Commands;
using GitTools.Common;

var debugOption = new Option<bool>("--debug", new[] { "--debug", "-d" });
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
    var quietOption = new Option<bool>("--quiet", new[] { "--quiet", "-q" });
    var noSwitchBranchOption = new Option<bool>("--no-switch-branch", new[] { "--no-switch-branch", "-n" });
    var useBranchOption = new Option<string?>("--use-branch", new[] { "--use-branch", "-b" });
    nukeCommand.Add(debugOption);
    nukeCommand.Add(quietOption);
    nukeCommand.Add(noSwitchBranchOption);
    nukeCommand.Add(useBranchOption);

    nukeCommand.SetAction(async (System.CommandLine.ParseResult parseResult) =>
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
    var quietOption = new Option<bool>("--quiet", new[] { "--quiet", "-q" });
    var targetBranchOption = new Option<string?>("--target-branch", new[] { "--target-branch", "-t" });
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
    var templateOption = new Option<string>("--template", new[] { "--template", "-t" });
    var defaultBranchOption = new Option<string>("--default-branch", new[] { "--default-branch", "-b" });
    var userEmailOption = new Option<string>("--user-email", new[] { "--user-email", "-e" });
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
