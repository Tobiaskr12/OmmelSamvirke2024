using FluentResults;

namespace TestHelpers;

public static class MockHelpers
{
    public static Task<Result<T>> FailedAsyncResult<T>()
    {
        return Task.FromResult(Result.Fail<T>(string.Empty));
    }
    
    public static Task<Result> FailedAsyncResult()
    {
        return Task.FromResult(Result.Fail(string.Empty));
    }

    public static Task<Result<T>> SuccessAsyncResult<T>(T returnValue)
    {
        return Task.FromResult(Result.Ok(returnValue));
    }
}
