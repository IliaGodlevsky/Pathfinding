namespace Pathfinding.Service.Interface;

public interface ISerializer<T>
{
    Task SerializeToAsync(T item, Stream stream,
        CancellationToken token = default);

    Task<T> DeserializeFromAsync(Stream stream,
        CancellationToken token = default);
}
