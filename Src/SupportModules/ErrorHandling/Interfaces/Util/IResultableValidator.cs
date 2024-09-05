using FluentResults;

namespace ErrorHandling.Interfaces.Util;

public interface IResultableValidator<T>
{
    Result<T> GetResult();
}
