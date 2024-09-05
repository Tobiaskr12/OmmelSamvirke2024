using ErrorHandling.Interfaces.TypeValidators;
using ErrorHandling.Interfaces.Util;

namespace ErrorHandling.Interfaces;

public interface IValueValidator : IValueValidatorForValueMethods;

public interface IValueValidatorStageTwoStringType : 
    IValueValidatorForValueMethods,
    IResultableValidator<string>,
    IStringValidator<IValueValidatorStageTwoStringType>;

public interface IValueValidatorStageTwoIntType : 
    IValueValidatorForValueMethods,
    IResultableValidator<int>,
    INumericValidator<IValueValidatorStageTwoIntType>;

public interface IValueValidatorStageTwoLongType : 
    IValueValidatorForValueMethods,
    IResultableValidator<long>,
    INumericValidator<IValueValidatorStageTwoLongType>;

public interface IValueValidatorStageTwoFloatType : 
    IValueValidatorForValueMethods,
    IResultableValidator<float>,
    INumericValidator<IValueValidatorStageTwoFloatType>;

public interface IValueValidatorStageTwoDoubleType : 
    IValueValidatorForValueMethods,
    IResultableValidator<double>,
    INumericValidator<IValueValidatorStageTwoDoubleType>;

public interface IValueValidatorStageTwoDecimalType : 
    IValueValidatorForValueMethods,
    IResultableValidator<decimal>,
    INumericValidator<IValueValidatorStageTwoDecimalType>;

public interface IValueValidatorStageTwoObjectType : 
    IValueValidatorForValueMethods,
    IResultableValidator<object>,
    IObjectValidator<IValueValidatorStageTwoObjectType>;

public interface IValueValidatorForValueMethods
{
    IStringValidator<IValueValidatorStageTwoStringType> ForValue(string value);
    INumericValidator<IValueValidatorStageTwoIntType> ForValue(int value);
    INumericValidator<IValueValidatorStageTwoLongType> ForValue(long value);
    INumericValidator<IValueValidatorStageTwoFloatType> ForValue(float value);
    INumericValidator<IValueValidatorStageTwoDoubleType> ForValue(double value);
    INumericValidator<IValueValidatorStageTwoDecimalType> ForValue(decimal value);
    IObjectValidator<IValueValidatorStageTwoObjectType> ForValue(object value);
}
