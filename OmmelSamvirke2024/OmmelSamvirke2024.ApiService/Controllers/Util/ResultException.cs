using FluentResults;

namespace OmmelSamvirke2024.ApiService.Controllers.Util;

public class ResultException : Exception
{
    public ResultBase Result { get; }

    public ResultException(ResultBase result)
    {
        Result = result;
    }
}