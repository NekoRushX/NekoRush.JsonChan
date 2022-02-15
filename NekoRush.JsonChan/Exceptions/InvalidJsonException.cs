namespace NekoRush.JsonChan.Exceptions;

public class InvalidJsonException : Exception
{
    public InvalidJsonException(string reason)
        : base($"Invalid JSON. {reason}")
    {
    }

    public InvalidJsonException(long position, string reason)
        : this($"At position: {position}, {reason}")
    {
    }
}
