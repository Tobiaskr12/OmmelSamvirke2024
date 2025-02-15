using FluentResults;

namespace ServiceModules.MediatorConfig.Exceptions;

public class ResultException : Exception
{
    public ResultBase Result { get; }

    public ResultException(ResultBase result)
    {
        Result = result;
    }
}
