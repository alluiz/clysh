namespace Clysh.Helper;

public abstract class ClyshIndexable<T> : IClyshIndexable<T>
{
    private T id = default!;

    public T Id
    {
        get => id;
        set => id = ValidatedId(value);
    }

    protected virtual T ValidatedId(T input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input), "Invalid id.");
            
        return input;
    }
}