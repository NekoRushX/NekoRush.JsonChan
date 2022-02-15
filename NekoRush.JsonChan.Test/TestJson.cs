using NUnit.Framework;

namespace NekoRush.JsonChan.Test;

public class Tests
{
    [Test]
    public void TestJson()
    {
        var json = Json.Parse(
            @"
{
    ""apple""  :  1,
    ""banana"" :  2,
    ""coco""   :  3
}
");

        // Asserts
        Assert.AreEqual(json.apple, 1);
        Assert.AreEqual(json.banana, 2);
        Assert.AreEqual(json.coco, 3);
        Assert.Pass();
    }

    [Test]
    public void TestJson2()
    {
        var json = Json.Parse(
            @"
{
    ""str"" : ""this is a string"",
    ""number"" : 3.1415926,
    ""boolean"": true
}
");

        // Asserts
        Assert.AreEqual(json.str, "this is a string");
        Assert.AreEqual(json.number, 3.1415926);
        Assert.AreEqual(json.boolean, true);
        Assert.Pass();
    }

    [Test]
    public void TestJson3()
    {
        var json = Json.Parse(
            @"
{
    ""str"" : ""this is a string"",
    ""number"" : 3.1415926,
    ""negative"" : -3.1415926,
    ""boolean"": true,
    ""j_obj"" : {
        ""str"": ""string \\in object"",
        ""number"" : 1000000000,
        ""boolean"": false
    },
    ""j_array"" : [
        ""apple"", ""banana"", ""coco""
    ]
}
");

        // Asserts
        Assert.AreEqual(json.str, "this is a string");
        Assert.AreEqual(json.number, 3.1415926);
        Assert.AreEqual(json.boolean, true);
        Assert.AreEqual(json.j_obj.str, "string \\in object");
        Assert.AreEqual(json.j_obj.number, 1000000000);
        Assert.AreEqual(json.j_obj.boolean, false);
        Assert.AreEqual(json.j_array[0], "apple");
        Assert.AreEqual(json.j_array[1], "banana");
        Assert.AreEqual(json.j_array[2], "coco");
        Assert.Pass();
    }

    [Test]
    public void TestJson4()
    {
        var json = Json.Parse(
@"
[
    ""apple"",
    ""banana"",
    ""coco""
]
");

        Assert.AreEqual(json[0], "apple");
        Assert.AreEqual(json[1], "banana");
        Assert.AreEqual(json[2], "coco");
        Assert.Pass();
    }
    
    [Test]
    public void TestJson5()
    {
        var json = Json.Parse("null");
        Assert.Pass();
    }
}
