using Pathfinding.Domain.Interface;

namespace Pathfinding.Infrastructure.Data.Extensions
{
    public static class VertexExtension
    {
        public static bool IsIsolated(this IVertex self)
        {
            return self?.IsObstacle == true
                || self?.Neighbors.All(vertex => vertex.IsObstacle) == true;
        }

        public static bool IsEqual(this IVertex self, IVertex vertex)
        {
            return self.Cost.CurrentCost.Equals(vertex.Cost.CurrentCost)
                && self.Position.Equals(vertex.Position)
                && self.IsObstacle == vertex.IsObstacle;
        }
    }
}