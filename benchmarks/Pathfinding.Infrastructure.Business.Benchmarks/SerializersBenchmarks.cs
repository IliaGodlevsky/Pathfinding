using BenchmarkDotNet.Attributes;
using Pathfinding.Infrastructure.Business.Benchmarks.Data;
using Pathfinding.Infrastructure.Business.Serializers;
using Pathfinding.Infrastructure.Business.Serializers.Decorators;

namespace Pathfinding.Infrastructure.Business.Benchmarks;

[MemoryDiagnoser]
public class SerializersBenchmarks
{
    private static Serializable toSerialize;

    [GlobalSetup]
    public void Setup()
    {
        toSerialize = new Serializable()
        {
            Size = 50,
            Cost = 123.54,
            Name = "Something in the way",
            Strength = 45.3f,
            Values = Enumerable.Range(0, 350).ToList()
        };
        for (int i = 0; i < 200; i++)
        {
            toSerialize.Serializables.Add(new Serializable()
            {
                Size = 50,
                Cost = 123.54,
                Name = "Something in the way",
                Strength = 45.3f,
                Values = Enumerable.Range(0, 350).ToList()
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
    public async Task BufferedCompressingSerializerBenchmark()
    {
        var serializer = new JsonSerializer<Serializable>();
        var buffer = new BufferedSerializer<Serializable>(serializer);
        var compress = new CompressSerializer<Serializable>(buffer);
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