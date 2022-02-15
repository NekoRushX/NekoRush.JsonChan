## JsonChan

A radical JSON parser library that parses JSON into a dynamic tree.   

Very simple to use, only need to code one line.  
Especially suit for some non-performance sensitivity scenes.

## Example
```C#
var json = Json.Parse(
@"{
  ""hello"": ""world"",
  ""fruits"": [
    ""apple"", ""banana"", ""coconut""
  ],
  ""number"": -3.1415926,
  ""boolean"": true
}");

// Prints 'world'
Console.WriteLine(json.hello);

// Prints '3'
Console.WriteLine(json.fruits.Count);

// Prints 'apple'
Console.WriteLine(json.fruits[0]);

```

## License
Licensed under MIT with ❤.
