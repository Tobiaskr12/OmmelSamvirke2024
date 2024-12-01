using FluentResults;

namespace OmmelSamvirke.DataAccess.Errors;

public class DatabaseError : Error
{
    public DatabaseError(string message) : base(message) { }
}

public class NotFoundError : Error
{
    public NotFoundError(string message) : base(message) { }
}
