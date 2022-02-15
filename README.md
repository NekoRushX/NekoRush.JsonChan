## NekoRush.JsonChan

A radical JSON parser library that parses JSON into a dynamic tree.   
It's very simple to use as only need to code one line to parse the JSON.   

**It's especially suited for some non-performance sensitivity scenes.**

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

## Performance
Platform:  

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1526 (21H2)  
Intel Core i9-10900X CPU 3.70GHz, 1 CPU, 20 logical and 10 physical cores  


### Small JSON file (in 283 KB)
> Auto generated random json.
```
|    Method |     Mean |     Error |    StdDev |
|---------- |---------:|----------:|----------:|
| BenchJson | 7.837 ms | 0.0784 ms | 0.0695 ms |
```

### Large JSON file (in 180 MB)
> Use [sf-city-lots-json](https://github.com/zemirco/sf-city-lots-json/blob/master/citylots.json) for testing.

```
|        Method |         Mean |       Error |      StdDev |
|-------------- |-------------:|------------:|------------:|
| BenchCityLots | 9,651.469 ms | 120.2081 ms | 112.4427 ms |

```

## License
Licensed under MIT with ❤.
