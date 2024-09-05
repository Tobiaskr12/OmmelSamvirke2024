using ErrorHandling.Interfaces.TypeValidators;
using ErrorHandling.Interfaces.Util;

namespace ErrorHandling.Interfaces;

public interface IClassValidator<T> : IResultableValidator<T> where T : class
{
    IClassValidatorStageTwo<T> ForClass(T validationClass);
}

public interface IClassValidatorStageTwo<T> : IPropertyValidatorMethods<T> where T : class;

public interface IClassValidatorStageThreeStringType<T> : 
    IPropertyValidatorMethods<T>,
    IResultableValidator<T>,
    IEndableValidator<IClassValidator<T>>,
    IStringValidator<IClassValidatorStageTwo<T>> where T : class;

public interface IClassValidatorStageThreeNumericType<T> : 
    IPropertyValidatorMethods<T>,
    IResultableValidator<T>,
    IEndableValidator<IClassValidator<T>>,
    INumericValidator<IClassValidatorStageTwo<T>> where T : class;

public interface IClassValidatorStageThreeObjectType<T> : 
    IPropertyValidatorMethods<T>,
    IResultableValidator<T>,
    IEndableValidator<IClassValidator<T>>,
    IObjectValidator<IClassValidatorStageTwo<T>> where T : class;

public interface IPropertyValidatorMethods<T> where T : class
{
    IStringValidator<IClassValidatorStageThreeStringType<T>> ForProperty(string value);
    INumericValidator<IClassValidatorStageThreeNumericType<T>> ForProperty(int value);
    IObjectValidator<IClassValidatorStageThreeObjectType<T>> ForProperty(object value);
}
