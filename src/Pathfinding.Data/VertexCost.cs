using Pathfinding.Domain.Interface;

namespace Pathfinding.Data;

public sealed class VertexCost(int cost) : IVertexCost
{
    public int CurrentCost { get; set; } = cost;

    public override int GetHashCode()
    {
        return CurrentCost.GetHashCode();
    }

    public IVertexCost DeepClone()
    {
        return new VertexCost(CurrentCost);
    }
}