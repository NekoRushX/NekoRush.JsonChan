using BenchmarkDotNet.Running;

namespace NekoRush.JsonChan.Bench;

public static class Program
{
    static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<JsonBench>();
        Console.WriteLine(summary);
        // new JsonBench().BenchJson();
    }
}
