using System.IO.Compression;

namespace Pathfinding.Service.Interface;

public interface ICsvSerializable
{
    void Serialize(ZipArchive archive);

    void Deserialize(ZipArchive archive);
}
