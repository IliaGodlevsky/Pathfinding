using Pathfinding.Infrastructure.Business.Algorithms.Exceptions;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Extensions;

public static class EnumerableExtensions
{
    public static IPathfindingVertex PopOrThrowDeadEndVertexException(this Stack<IPathfindingVertex> stack)
    {
        return stack.Count == 0 ? throw new DeadendVertexException() : stack.Pop();
    }

    public static IPathfindingVertex DequeueOrThrowDeadEndVertexException(this Queue<IPathfindingVertex> queue)
    {
        return queue.Count == 0 ? throw new DeadendVertexException() : queue.Dequeue();
    }

    public static IPathfindingVertex FirstOrNullVertex(this IEnumerable<IPathfindingVertex> collection,
        Func<IPathfindingVertex, bool> predicate)
    {
        return collection.FirstOrDefault(predicate) ?? NullPathfindingVertex.Interface;
    }
}