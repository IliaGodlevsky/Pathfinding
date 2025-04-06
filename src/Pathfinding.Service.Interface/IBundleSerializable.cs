using System.IO.Compression;

namespace Pathfinding.Service.Interface;

public interface IBundleSerializable
{
    void Serialize(ZipArchive bundle);

    void Deserialize(ZipArchive bundle);
}
