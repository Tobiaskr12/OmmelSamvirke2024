namespace ErrorHandling.Interfaces.Util;

public interface IEndableValidator<out T>
{
    T End();
}
