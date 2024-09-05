using FluentResults;

namespace ErrorHandling.Interfaces.Util;

public interface IResultableValidator
{
    Result<T> GetResult<T>();
}
