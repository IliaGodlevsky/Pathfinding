using Pathfinding.Service.Interface;

namespace Pathfinding.Service.Extensions;

public static class VertexExtensions
{
    public static bool IsNeighbor(this IPathfindingVertex self, IPathfindingVertex candidate)
    {
        return self.Neighbors.Contains(candidate);
    }
}