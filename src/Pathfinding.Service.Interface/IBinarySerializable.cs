namespace Pathfinding.Service.Interface;

public interface IBinarySerializable
{
    Task SerializeAsync(Stream stream, CancellationToken token = default);

    Task DeserializeAsync(Stream stream, CancellationToken token = default);
}
