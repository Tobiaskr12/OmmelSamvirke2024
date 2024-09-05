using ErrorHandling.Interfaces.TypeValidators;
using ErrorHandling.Interfaces.Util;

namespace ErrorHandling.Interfaces;

public interface IValueValidator : IValueValidatorForValueMethods;

public interface IValueValidatorStageTwoStringType : 
    IValueValidatorForValueMethods,
    IResultableValidator,
    IStringValidator<IValueValidatorStageTwoStringType>;

public interface IValueValidatorStageTwoNumericType : 
    IValueValidatorForValueMethods,
    IResultableValidator,
    INumericValidator<IValueValidatorStageTwoNumericType>;

public interface IValueValidatorStageTwoObjectType : 
    IValueValidatorForValueMethods,
    IResultableValidator,
    IObjectValidator<IValueValidatorStageTwoObjectType>;

public interface IValueValidatorForValueMethods
{
    IStringValidator<IValueValidatorStageTwoStringType> ForValue(string value);
    INumericValidator<IValueValidatorStageTwoNumericType> ForValue(int value);
    INumericValidator<IValueValidatorStageTwoObjectType> ForValue(object value);
}
