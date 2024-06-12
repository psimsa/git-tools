using GitNuke;

var printGitOutput = args.Any(a => a is "--debug" or "-d");
var requestConfirmation = !args.Any(a => a is "--quiet" or "-q");
var noSwitchBranch = args.Any(a => a is "--no-switch-branch" or "-n");

var p = new Processor(printGitOutput, requestConfirmation, noSwitchBranch);
var result = await p.Run();

if (result.IsFailure)
{
    Console.WriteLine($"Error: {result.Error}");
    return 1;
}

Console.WriteLine("Git repo nuked.");
return 0;