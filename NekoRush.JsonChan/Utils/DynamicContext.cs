using System.Dynamic;

// ReSharper disable ConvertToAutoProperty
// ReSharper disable MemberCanBePrivate.Global

namespace NekoRush.JsonChan.Utils;

internal class DynamicContext
{
    public dynamic Value => _value;

    private dynamic _value;
    private readonly Stack<dynamic> _contextPath;

    public DynamicContext()
    {
        _value = new ExpandoObject();
        _contextPath = new();
    }

    /// <summary>
    /// Push context
    /// </summary>
    public void PushContext(string name)
    {
        // Create a subtree
        if (name == string.Empty)
        {
            _contextPath.Push(_contextPath.Count == 0 ? _value : new ExpandoObject());
            return;
        }

        var context = new ExpandoObject();

        if (IsArrayContext())
        {
            PutArrayValue(context);
            _contextPath.Push(context);
            return;
        }

        PutValue(name, context);
        _contextPath.Push(context);
    }

    /// <summary>
    /// Pop context
    /// </summary>
    public void PopContext()
    {
        if (_contextPath.Count > 1) _contextPath.Pop();
    }

    public void PushArrayContext(string name)
    {
        var context = new List<dynamic>();

        // Create a subtree
        if (name == string.Empty)
        {
            if (_contextPath.Count == 0) _value = context;
            _contextPath.Push(context);
            return;
        }

        PutValue(name, context);
        _contextPath.Push(context);
    }

    public bool IsArrayContext()
        => _contextPath.Peek() is List<dynamic>;

    /// <summary>
    /// Put object value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void PutValue(string key, dynamic value)
    {
        if (!IsArrayContext())
            (_contextPath.Peek() as IDictionary<string, object>)![key] = value;
        else PutArrayValue(value);
    }

    /// <summary>
    /// Put array value
    /// </summary>
    /// <param name="value"></param>
    public void PutArrayValue(dynamic value)
    {
        (_contextPath.Peek() as List<dynamic>)!.Add(value);
    }
}

internal class Stack<T> : List<T>
{
    public void Push(T item) => Insert(0, item);

    public T Pop()
    {
        if (Count <= 0) return default!;

        var item = this[0];
        RemoveAt(0);
        return item;
    }

    public T Peek()
    {
        return Count > 0 ? this[0] : default!;
    }
}
