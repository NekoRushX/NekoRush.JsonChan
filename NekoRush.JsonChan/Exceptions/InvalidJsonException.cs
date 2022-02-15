namespace NekoRush.JsonChan.Exceptions;

/// <summary>
/// Invalid JSON Exception
/// </summary>
public class InvalidJsonException : Exception
{
    /// <summary>
    /// Create exception
    /// </summary>
    /// <param name="reason">reason string</param>
    public InvalidJsonException(string reason)
        : base($"Invalid JSON. {reason}")
    {
    }

    /// <summary>
    /// Create exception
    /// </summary>
    /// <param name="position">stream position</param>
    /// <param name="reason">the reason</param>
    public InvalidJsonException(long position, string reason)
        : this($"At position: {position}, {reason}")
    {
    }
}
