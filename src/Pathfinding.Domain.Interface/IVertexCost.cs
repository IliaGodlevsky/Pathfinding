using Pathfinding.Shared.Interface;

namespace Pathfinding.Domain.Interface;

public interface IVertexCost : ICloneable<IVertexCost>
{
    int CurrentCost { get; set; }
}
