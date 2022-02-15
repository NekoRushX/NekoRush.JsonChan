using System.Text;
using NekoRush.JsonChan.Exceptions;
using NekoRush.JsonChan.Utils;

// ReSharper disable RedundantAssignment
// ReSharper disable MemberCanBePrivate.Global

namespace NekoRush.JsonChan;

internal class JsonParser
{
    [Flags]
    private enum ParseStatus
    {
        ExceptObjStart = 1, /*    {  */
        ExceptObjEnd = 2, /*      }  */
        ExceptArrayStart = 4, /*  [  */
        ExceptArrayEnd = 8, /*    ]  */
        ExceptComma = 16, /*      ,  */
        ExceptQuote = 32, /*      "  */
        ExceptColon = 64 /*       :  */
    }

    private readonly Stream _jsonStream;
    private readonly DynamicContext _dynamicContext;
    private readonly bool _needDispose;

    public JsonParser(Stream json)
    {
        _jsonStream = json;
        _dynamicContext = new();
    }

    ~JsonParser()
    {
        // Destroy the stream if it
        // was created from string or byte[]
        if (_needDispose) _jsonStream.Dispose();
    }

    public JsonParser(IEnumerable<byte> json)
        : this(new MemoryStream(json.ToArray()))
    {
        _needDispose = true;
    }

    public JsonParser(string json)
        : this(Encoding.UTF8.GetBytes(json))
    {
    }

    public dynamic Parse()
    {
        // Asserts the stream can read
        if (!_jsonStream.CanRead)
            throw new InvalidJsonException("Stream cannot read.");

        // Asserts the stream can seek
        if (!_jsonStream.CanSeek)
            throw new InvalidJsonException("Stream cannot seek.");
        _jsonStream.Seek(0, SeekOrigin.Begin);

        var currExcept = ParseStatus.ExceptObjStart | ParseStatus.ExceptArrayStart;
        var currString = string.Empty;
        var needValue = false;

        do
        {
            // Read the token
            var currChar = _jsonStream.ReadByte();
            switch (currChar)
            {
                case < 0:
                    goto ret;

                // Ignore all white characters
                case ' ':
                case '\r':
                case '\n':
                case '\t':
                    continue;

                // Push object context
                case '{':
                    AssertExcepts(currExcept, ParseStatus.ExceptObjStart);
                    _dynamicContext.PushContext(currString);
                    needValue = false;
                    currExcept = ParseStatus.ExceptQuote | ParseStatus.ExceptArrayStart | ParseStatus.ExceptObjStart | ParseStatus.ExceptObjEnd;
                    break;

                // Pop object context
                case '}':
                    AssertExcepts(currExcept, ParseStatus.ExceptObjEnd);

                    // Pop context 
                    _dynamicContext.PopContext();
                    currExcept = ParseStatus.ExceptComma | ParseStatus.ExceptArrayEnd | ParseStatus.ExceptObjEnd;
                    break;

                case '[':
                    AssertExcepts(currExcept, ParseStatus.ExceptArrayStart);
                    _dynamicContext.PushArrayContext(currString);
                    needValue = false;
                    currExcept = ParseStatus.ExceptQuote | ParseStatus.ExceptArrayStart | ParseStatus.ExceptObjStart | ParseStatus.ExceptArrayEnd;
                    break;

                case ']':
                    AssertExcepts(currExcept, ParseStatus.ExceptArrayEnd);

                    // Pop context 
                    _dynamicContext.PopContext();
                    currExcept = ParseStatus.ExceptComma | ParseStatus.ExceptArrayEnd | ParseStatus.ExceptObjEnd;
                    break;

                // Parse the next field
                case ',':
                    AssertExcepts(currExcept, ParseStatus.ExceptComma);
                    currExcept = ParseStatus.ExceptQuote | ParseStatus.ExceptArrayStart | ParseStatus.ExceptObjStart;
                    break;

                // Parse field value
                case ':':
                    AssertExcepts(currExcept, ParseStatus.ExceptColon);
                    needValue = true;
                    currExcept = ParseStatus.ExceptQuote | ParseStatus.ExceptArrayStart | ParseStatus.ExceptObjStart;
                    break;

                // Parse strings
                case '"':
                    AssertExcepts(currExcept, ParseStatus.ExceptQuote);

                    if (needValue)
                    {
                        needValue = false;
                        _dynamicContext.PutValue(currString, ParseString());
                    }
                    else if (_dynamicContext.IsArrayContext())
                    {
                        _dynamicContext.PutArrayValue(ParseString());
                    }
                    else currString = ParseString();

                    currExcept = ParseStatus.ExceptComma | ParseStatus.ExceptArrayEnd | ParseStatus.ExceptObjEnd | ParseStatus.ExceptColon;
                    break;

                // Parse Numbers
                case '-':
                case >= '0' and <= '9':
                    _jsonStream.Seek(-1, SeekOrigin.Current);

                    if (needValue)
                    {
                        needValue = false;
                        _dynamicContext.PutValue(currString, ParseNumber());
                    }
                    else if (_dynamicContext.IsArrayContext())
                    {
                        _dynamicContext.PutArrayValue(ParseNumber());
                    }
                    else currString = ParseString();

                    currExcept = ParseStatus.ExceptComma | ParseStatus.ExceptArrayEnd | ParseStatus.ExceptObjEnd;
                    break;

                // Parse boolean
                case 't':
                case 'f':

                    if (needValue)
                    {
                        needValue = false;
                        _dynamicContext.PutValue(currString, ParseBoolean());
                    }
                    else if (_dynamicContext.IsArrayContext())
                    {
                        _dynamicContext.PutArrayValue(ParseBoolean());
                    }

                    currExcept = ParseStatus.ExceptComma | ParseStatus.ExceptArrayEnd | ParseStatus.ExceptObjEnd;

                    break;

                case 'n':

                    // Compare remain characters (null)
                    if (ReadAndCompare(3, "ull"))
                    {
                        if (needValue)
                        {
                            needValue = false;
                            _dynamicContext.PutValue(currString, null!);
                        }
                        else if (_dynamicContext.IsArrayContext())
                        {
                            _dynamicContext.PutArrayValue(null!);
                        }
                    }

                    // Throw the exception
                    else goto default;

                    currExcept = ParseStatus.ExceptComma | ParseStatus.ExceptArrayEnd | ParseStatus.ExceptObjEnd;
                    break;

                default: throw new InvalidJsonException(_jsonStream.Position, "Unexpected token.");
            }
        } while (_jsonStream.Length > 0);

        ret:
        return _dynamicContext.Value;
    }

    private dynamic ParseNumber()
    {
        var list = new List<char>();
        var charBuf = 0;
        var dotCount = 0;
        var subCount = 0;

        do
        {
            charBuf = _jsonStream.ReadByte();

            switch (charBuf)
            {
                // EOF
                case -1: break;

                case '.':
                    if (list.Count == 0 || dotCount > 1)
                        throw new InvalidJsonException(_jsonStream.Position, "Unexpected number type.");
                    list.Add('.');
                    dotCount++;
                    break;

                case '-':
                    if (subCount > 1)
                        throw new InvalidJsonException(_jsonStream.Position, "Unexpected number type.");
                    list.Add('-');
                    subCount++;
                    break;

                case >= '0' and <= '9':
                    list.Add((char) charBuf);
                    break;

                default:
                    var str = new string(list.ToArray());
                    _jsonStream.Seek(-1, SeekOrigin.Current);

                    if (subCount > 0 && !str.StartsWith('-'))
                        throw new InvalidJsonException(_jsonStream.Position, "Unexpected number type.");

                    // Parse number 
                    if (dotCount == 0 && subCount == 0) return ulong.Parse(str);
                    if (dotCount == 0 && subCount != 0) return long.Parse(str);
                    return double.Parse(str);
            }
        } while (charBuf != -1);

        throw new InvalidJsonException(_jsonStream.Position, "Unexpected EOF.");
    }

    private bool ParseBoolean()
    {
        if (ReadAndCompare(3, "rue")) return true;
        if (ReadAndCompare(4, "alse")) return false;

        throw new InvalidJsonException(_jsonStream.Position, "Unexpected boolean type.");
    }

    private string ParseString()
    {
        var list = new List<byte>();
        var charBuf = 0;

        do
        {
            charBuf = _jsonStream.ReadByte();

            switch (charBuf)
            {
                // EOF
                case -1: break;

                case '\\':
                    switch (_jsonStream.ReadByte())
                    {
                        case '"':
                            list.Add((byte) '"');
                            break;

                        case '\\':
                            list.Add((byte) '\\');
                            break;
                    }

                    break;

                case '"':
                    return Encoding.UTF8.GetString(list.ToArray());

                default:
                    list.Add((byte) charBuf);
                    break;
            }
        } while (charBuf != -1);

        throw new InvalidJsonException(_jsonStream.Position, "Unexpected EOF.");
    }

    /// <summary>
    /// Read stream and compare the string
    /// </summary>
    /// <param name="length">Intended to read</param>
    /// <param name="chars">Chars to compare</param>
    /// <returns></returns>
    /// <exception cref="InvalidJsonException"></exception>
    private bool ReadAndCompare(int length, string chars)
    {
        if (_jsonStream.Length < length)
            throw new InvalidJsonException(_jsonStream.Position, "Unexpected EOF.");

        var charBuf = new byte[length];
        _jsonStream.Read(charBuf, 0, length);

        var result = chars == Encoding.UTF8.GetString(charBuf);
        if (!result) _jsonStream.Seek(-length, SeekOrigin.Current);
        return result;
    }

    /// <summary>
    /// Assert the token excepts in the next
    /// </summary>
    /// <param name="current">Current token status</param>
    /// <param name="excepted">Excepted token</param>
    /// <exception cref="InvalidJsonException"></exception>
    private void AssertExcepts(ParseStatus current, ParseStatus excepted)
    {
        if ((current & excepted) == 0)
            throw new InvalidJsonException(_jsonStream.Position, "Unexpected token.");

        // assert ok
    }
}
