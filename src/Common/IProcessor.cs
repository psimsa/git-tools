using CSharpFunctionalExtensions;

namespace GitTools.Common;

public interface IProcessor
{
    Task<Result> Run();
}
