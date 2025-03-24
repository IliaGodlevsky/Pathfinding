using Pathfinding.Shared.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Domain.Interface;

public interface IVertexCost : ICloneable<IVertexCost>
{
    InclusiveValueRange<int> CostRange { get; set; }

    int CurrentCost { get; set; }
}
