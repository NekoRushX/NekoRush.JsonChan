using BenchmarkDotNet.Attributes;

namespace NekoRush.JsonChan.Bench;

public class JsonBench
{
    [Benchmark]
    public string BenchRandom()
    {
        Json.Parse(File.OpenRead("../../../../../../../BenchFiles/random.json"));
        return "finish";
    }
    
    [Benchmark]
    public string BenchCityLots()
    {
        Json.Parse(File.OpenRead("../../../../../../../BenchFiles/sf-city-lots-json/citylots.json"));
        return "finish";
    }
}
