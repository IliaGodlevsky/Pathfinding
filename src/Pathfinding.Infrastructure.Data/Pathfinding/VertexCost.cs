using Pathfinding.Domain.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Data.Pathfinding;

public sealed class VertexCost(int cost, InclusiveValueRange<int> costRange) : IVertexCost
{
    public InclusiveValueRange<int> CostRange { get; set; } = costRange;

    public int CurrentCost { get; set; } = cost;

    public override int GetHashCode()
    {
        return CurrentCost.GetHashCode();
    }

    public IVertexCost DeepClone()
    {
        return new VertexCost(CurrentCost, CostRange);
    }
}