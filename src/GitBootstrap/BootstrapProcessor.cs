using Common;
using CSharpFunctionalExtensions;

namespace GitBootstrap;

public class BootstrapProcessor(bool printGitOutput) : IProcessor
{
    public async Task<Result> Run()
    {
        return Result.Success();
    }
}
