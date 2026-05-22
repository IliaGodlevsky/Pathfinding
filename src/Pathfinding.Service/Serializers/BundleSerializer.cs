using Pathfinding.Service.Interface;
using Pathfinding.Service.Serializers.Exceptions;
using System.IO.Compression;

namespace Pathfinding.Service.Serializers;

public sealed class BundleSerializer<T> : ISerializer<T>
    where T : IBundleSerializable, new()
{
    public async Task<T> DeserializeFromAsync(Stream stream,
        CancellationToken token = default)
    {
        try
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
            var result = new T();
            await result.DeserializeAsync(archive, token).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            throw new SerializationException(ex.Message, ex);
        }
    }

    public async Task SerializeToAsync(T item,
        Stream stream, CancellationToken token = default)
    {
        try
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true);
            await item.SerializeAsync(archive, token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new SerializationException(ex.Message, ex);
        }
    }
}