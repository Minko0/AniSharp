namespace AniSharp.Console;

public struct Optional<T>(T value) : IEquatable<Optional<T>>
{
    public bool HasValue { get; private set; } = true;
    private readonly T _value = value;
    public T Value
    {
        get
        {
            if (HasValue)
            {
                return _value;
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
        if (obj is Optional<T> optional)
        {
            return Equals(optional);
        }

        return false;
    }
    public bool Equals(Optional<T> other)
    {
        if (HasValue && other.HasValue)
        {
            return Equals(_value, other._value);
        }

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

    public override int GetHashCode()
    {
        return HashCode.Combine(_value, HasValue);
    }
}