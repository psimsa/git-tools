using System.CommandLine;
using GitTools.Commands;
using GitTools.Common;

var rootCommand = new RootCommand("Suite of small git utilities");

var debugOption = new Option<bool>(["--debug", "-d"], () => false, "Show git command output");
rootCommand.AddGlobalOption(debugOption);

SetupNukeCommand(rootCommand, debugOption);

SetupBootstrapCommand(rootCommand, debugOption);

SetupTidyBranchCommand(rootCommand, debugOption);

await rootCommand.InvokeAsync(args);

static void SetupNukeCommand(RootCommand rootCommand, Option<bool> debugOption)
{
    var nukeCommand = new Command("nuke", "Clean-up current repo");

    var quietOption = new Option<bool>(
        ["--quiet", "-q"],
        () => false,
        "Do not ask for confirmation"
    );
    nukeCommand.AddOption(quietOption);

    var noSwitchBranchOption = new Option<bool>(
        ["--no-switch-branch", "-n"],
        () => false,
        "Do not switch to main or master branch"
    );
    nukeCommand.AddOption(noSwitchBranchOption);

    var useBranchOption = new Option<string?>(
        new[] { "--use-branch", "-b" },
        () => null,
        "Branch to use instead of master/main branch"
    );
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

    var quietOption = new Option<bool>(
        ["--quiet", "-q"],
        () => false,
        "Do not ask for confirmation"
    );
    tidyBranchCommand.AddOption(quietOption);

    var targetBranchOption = new Option<string?>(
        new[] { "--target-branch", "-t" },
        () => null,
        "Branch to rebase onto"
    );
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

    var templateOption = new Option<string>(new[] { "--template", "-t" }, "Template to use")
    {
        IsHidden = true,
    };
    bootstrapCommand.AddOption(templateOption);
    var defaultBranchOption = new Option<string>(
        new[] { "--default-branch", "-b" },
        () => "main",
        "Default branch to use"
    );
    bootstrapCommand.AddOption(defaultBranchOption);
    var userEmailOption = new Option<string>(new[] { "--user-email", "-e" }, "User email to use");
    bootstrapCommand.AddOption(userEmailOption);

    bootstrapCommand.SetHandler(BootstrapCommand.Run,
        debugOption,
        templateOption,
        defaultBranchOption,
        userEmailOption
    );
    rootCommand.Add(bootstrapCommand);
}
