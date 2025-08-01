using System.CommandLine;
using GitTools.Commands;
using GitTools.Common;

var debugOption = new Option<bool>(["--debug", "-d"], () => false, "Show git command output");
var rootCommand = new RootCommand("Suite of small git utilities");
rootCommand.AddGlobalOption(debugOption);

SetupNukeCommand(rootCommand, debugOption);
SetupBootstrapCommand(rootCommand, debugOption);
SetupTidyBranchCommand(rootCommand, debugOption);

return await rootCommand.InvokeAsync(args);

static void SetupNukeCommand(RootCommand rootCommand, Option<bool> debugOption)
{
    var nukeCommand = new Command("nuke", "Clean-up current repo");
    var quietOption = new Option<bool>(["--quiet", "-q"], () => false, "Do not ask for confirmation");
    var noSwitchBranchOption = new Option<bool>(["--no-switch-branch", "-n"], () => false, "Do not switch to main or master branch");
    var useBranchOption = new Option<string?>(["--use-branch", "-b"], () => null, "Branch to use instead of master/main branch");
    nukeCommand.AddOption(quietOption);
    nukeCommand.AddOption(noSwitchBranchOption);
    nukeCommand.AddOption(useBranchOption);

    nukeCommand.SetHandler(NukeCommand.Run,
        debugOption,
        quietOption,
        noSwitchBranchOption,
        useBranchOption
    );
    rootCommand.Add(nukeCommand);
}

static void SetupTidyBranchCommand(RootCommand rootCommand, Option<bool> debugOption)
{
    var tidyBranchCommand = new Command(
        "tidy-branch",
        "Create single squashed commit from current branch rebased onto another branch (master/main by default)"
    );
    var quietOption = new Option<bool>(["--quiet", "-q"], () => false, "Do not ask for confirmation");
    var targetBranchOption = new Option<string?>(["--target-branch", "-t"], () => null, "Branch to rebase onto");
    tidyBranchCommand.AddOption(quietOption);
    tidyBranchCommand.AddOption(targetBranchOption);

    tidyBranchCommand.SetHandler(TidyBranchCommand.Run,
        debugOption,
        quietOption,
        targetBranchOption
    );
    rootCommand.Add(tidyBranchCommand);
}

static void SetupBootstrapCommand(RootCommand rootCommand, Option<bool> debugOption)
{
    var bootstrapCommand = new Command("bootstrap", "Bootstrap repo from parameters");
    var templateOption = new Option<string>(["--template", "-t"], "Template to use")
    {
        IsHidden = true,
    };
    var defaultBranchOption = new Option<string>(["--default-branch", "-b"], () => "main", "Default branch to use");
    var userEmailOption = new Option<string>(["--user-email", "-e"], "User email to use");
    bootstrapCommand.AddOption(templateOption);
    bootstrapCommand.AddOption(defaultBranchOption);
    bootstrapCommand.AddOption(userEmailOption);

    bootstrapCommand.SetHandler(BootstrapCommand.Run,
        debugOption,
        templateOption,
        defaultBranchOption,
        userEmailOption
    );
    rootCommand.Add(bootstrapCommand);
}
