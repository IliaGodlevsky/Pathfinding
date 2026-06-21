using BenchmarkDotNet.Attributes;
using Pathfinding.Serialization;
using Pathfinding.Serialization.Decorators;
using Pathfinding.Service.Benchmarks.Data;

namespace Pathfinding.Service.Benchmarks;

[MemoryDiagnoser]
public class SerializersBenchmarks
{
    private static Serializable toSerialize;

    [GlobalSetup]
    public static void Setup()
    {
        toSerialize = new Serializable()
        {
            Size = 50,
            Cost = 123.54,
            Name = "Something in the way",
            Strength = 45.3f,
            Values = [.. Enumerable.Range(0, 350)]
        };
        for (int i = 0; i < 200; i++)
        {
            toSerialize.Serializables.Add(new Serializable()
            {
                Size = 50,
                Cost = 123.54,
                Name = "Something in the way",
                Strength = 45.3f,
                Values = [.. Enumerable.Range(0, 350)]
            });
        }
    }

    [Benchmark]
    public async Task CompressingSerializerBenchmark()
    {
        var serializer = new JsonSerializer<Serializable>();
        var compress = new CompressSerializer<Serializable>(serializer);
        var memory = new MemoryStream();
        await compress.SerializeToAsync(toSerialize, memory);
    }

    [Benchmark]
    public async Task RegularSerializerBenchmark()
    {
        var serializer = new JsonSerializer<Serializable>();
        var memory = new MemoryStream();
        await serializer.SerializeToAsync(toSerialize, memory);
    }

    [Benchmark(Baseline = true)]
    public async Task RegularBinarySerializerBenchmark()
    {
        var serializer = new BinarySerializer<Serializable>();
        var memory = new MemoryStream();
        await serializer.SerializeToAsync(toSerialize, memory);
    }
}