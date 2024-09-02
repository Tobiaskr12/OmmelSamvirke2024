using OmmelSamvirke.ErrorHandling.Models;

namespace OmmelSamvirke.ErrorHandling.Interfaces;

public interface IValidator
{
    IValidator ValidateLength(string value, int minLength, int maxLength, Enum errorCode);
    IValidator ValidateRequired(string value, Enum errorCode);
    bool IsSuccess();
    int ErrorCount();
    List<Error> GetErrors();
}
