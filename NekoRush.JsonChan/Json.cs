using NekoRush.JsonChan.Exceptions;

namespace NekoRush.JsonChan;

public static class Json
{
    /// <summary>
    /// Parse the JSON string to dynamic tree
    /// </summary>
    /// <param name="string"></param>
    /// <returns></returns>
    /// <exception cref="InvalidJsonException"></exception>
    public static dynamic Parse(string @string)
    {
        // Parse to dynamic tree
        var parser = new JsonParser(@string);
        return parser.Parse();
    }
    
    /// <summary>
    /// Parse the JSON string to dynamic tree
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    /// <exception cref="InvalidJsonException"></exception>
    public static dynamic Parse(Stream stream)
    {
        // Parse to dynamic tree
        var parser = new JsonParser(stream);
        return parser.Parse();
    }
}
