using FluentResults;

namespace OmmelSamvirke2024.Api.Controllers.Util;

public class ResultException : Exception
{
    public ResultBase Result { get; }

    public ResultException(ResultBase result)
    {
        Result = result;
    }
}
