using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Extensions;

namespace Pathfinding.Infrastructure.Business.Benchmarks.Data;

public sealed class Serializable : IBinarySerializable
{
    public int Size { get; set; }

    public string Name { get; set; }

    public double Cost { get; set; }

    public double Strength { get; set; }

    public List<int> Values { get; set; } = [];

    public List<Serializable> Serializables { get; set; } = [];

    public async Task DeserializeAsync(Stream stream, CancellationToken token = default)
    {
        Size = await stream.ReadInt32Async(token).ConfigureAwait(false);
        Cost = await stream.ReadDoubleAsync(token).ConfigureAwait(false);
        Name = await stream.ReadStringAsync(token).ConfigureAwait(false);
        Strength = await stream.ReadDoubleAsync(token).ConfigureAwait(false);
        Values = [.. await stream.ReadArrayAsync(token).ConfigureAwait(false)];
        Serializables = [.. await stream.ReadSerializableArrayAsync<Serializable>(token).ConfigureAwait(false)];
    }

    public async Task SerializeAsync(Stream stream, CancellationToken token = default)
    {
        await stream.WriteInt32Async(Size, token).ConfigureAwait(false);
        await stream.WriteDoubleAsync(Cost, token).ConfigureAwait(false);
        await stream.WriteStringAsync(Name, token).ConfigureAwait(false);
        await stream.WriteDoubleAsync(Strength, token).ConfigureAwait(false);
        await stream.WriteInt32ArrayAsync(Values, token).ConfigureAwait(false);
        await stream.WriteInt32Async(Size, token).ConfigureAwait(false);
        await stream.WriteAsync(Serializables, token).ConfigureAwait(false);
    }
}