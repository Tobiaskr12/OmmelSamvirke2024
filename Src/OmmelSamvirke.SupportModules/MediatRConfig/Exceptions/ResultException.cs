using FluentResults;

namespace OmmelSamvirke.SupportModules.MediatRConfig.Exceptions;

public class ResultException : Exception
{
    public ResultBase Result { get; }

    public ResultException(ResultBase result)
    {
        Result = result;
    }
}
