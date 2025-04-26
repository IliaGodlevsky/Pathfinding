using Pathfinding.Infrastructure.Business.Algorithms.Exceptions;
using Pathfinding.Service.Interface;
using Priority_Queue;

namespace Pathfinding.Infrastructure.Business.Extensions;

public static class SimplePriorityQueueExtensions
{
    public static double GetPriorityOrInfinity(this SimplePriorityQueue<IPathfindingVertex, double> self, IPathfindingVertex vertex)
    {
        return self.TryGetPriority(vertex, out var value) ? value : double.PositiveInfinity;
    }

    public static IPathfindingVertex TryFirstOrThrowDeadEndVertexException(this SimplePriorityQueue<IPathfindingVertex, double> self)
    {
        return self.TryFirst(out var vertex) ? vertex : throw new DeadendVertexException();
    }
}