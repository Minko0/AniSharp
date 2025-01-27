namespace AniSharp.Console;

public struct ClientResponse<T>(T value) : IEquatable<ClientResponse<T>>
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

    public static explicit operator T(ClientResponse<T> clientResponse)
    {
        return clientResponse.Value;
    }
    public static implicit operator ClientResponse<T>(T value)
    {
        return new ClientResponse<T>(value);
    }

    public override bool Equals(object obj)
    {
        if (obj is ClientResponse<T> optional)
        {
            return Equals(optional);
        }

        return false;
    }
    public bool Equals(ClientResponse<T> other)
    {
        if (HasValue && other.HasValue)
        {
            return Equals(_value, other._value);
        }

        return HasValue == other.HasValue;
    }

    public static bool operator ==(ClientResponse<T> left, ClientResponse<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ClientResponse<T> left, ClientResponse<T> right)
    {
        return !(left == right);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_value, HasValue);
    }
}