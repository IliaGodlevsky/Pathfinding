using System.IO.Compression;

namespace Pathfinding.Service.Interface;

public interface IBundleSerializable
{
    Task SerializeAsync(ZipArchive bundle, CancellationToken token = default);

    Task DeserializeAsync(ZipArchive bundle, CancellationToken token = default);
}
