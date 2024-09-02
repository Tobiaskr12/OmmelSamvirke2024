using FluentResults;

namespace OmmelSamvirke2024.Api.Controllers.Util;

public static class ResultHelper
{
    public static T ThrowIfResultIsFailed<T>(Result<T> result)
    {
        {
            if (result.IsSuccess) return result.Value; 
            
            throw new ResultException(Result.Fail(result.Errors));
        }
    }
}
