namespace Jelly;

public static class Ref
{
    internal sealed class __;

    public static Ref<T> Create<T>(T baseValue = default)
    {
        return (value) => {
            if(value != default(__) && value is T t)
            {
                baseValue = t;
            }
            return baseValue;
        };
    }

    public static Ref<T> From<T>(ref T value) => Create(value);
}

public delegate T Ref<T>(object value = default(Ref.__));
