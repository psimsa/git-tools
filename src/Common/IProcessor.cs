using CSharpFunctionalExtensions;

namespace Common;

public interface IProcessor
{
    Task<Result> Run();
}
