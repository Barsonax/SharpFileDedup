using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

namespace SharpDedup.Console;

[MemoryDiagnoser]
[EtwProfiler]
public class BasicBenchmark
{
    private string[] paths;
    
    [GlobalSetup]
    public void Setup()
    {
        var content = new byte[1000000];

        File.WriteAllBytes("data", content);

        paths = new string[10000];
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i] = $"data{i}";
            File.Copy("data", paths[i], true);
        }
    }
    
    [Benchmark]
    public async Task<Stream> Foo()
    {
        var memoryStream = new MemoryStream();
        await DeduplicationSerializer.Serialize(memoryStream, paths);

        return memoryStream;
    }
}