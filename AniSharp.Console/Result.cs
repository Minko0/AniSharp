namespace AniSharp.Console;

public struct Optional<T>(T value)
{
    public bool HasValue { get; private set; } = true;
    private T value = value;
    public T Value
    {
        get
        {
            if (HasValue)
            {
                return value;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }

    public static explicit operator T(Optional<T> optional)
    {
        return optional.Value;
    }
    public static implicit operator Optional<T>(T value)
    {
        return new Optional<T>(value);
    }

    public override bool Equals(object obj)
    {
        if (obj is Optional<T>)
            return this.Equals((Optional<T>)obj);
        else
            return false;
    }
    public bool Equals(Optional<T> other)
    {
        if (HasValue && other.HasValue)
            return object.Equals(value, other.value);
        else
            return HasValue == other.HasValue;
    }

    public static bool operator ==(Optional<T> left, Optional<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Optional<T> left, Optional<T> right)
    {
        return !(left == right);
    }
}