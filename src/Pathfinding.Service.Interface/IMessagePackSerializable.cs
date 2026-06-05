namespace Pathfinding.Service.Interface;

public interface IMessagePackSerializable
{
    Task SerializePackAsync(Stream stream, CancellationToken token = default);

    Task DeserializePackAsync(Stream stream, CancellationToken token = default);
}
