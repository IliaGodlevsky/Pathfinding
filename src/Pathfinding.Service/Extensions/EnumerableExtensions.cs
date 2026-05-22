using Pathfinding.Data;
using Pathfinding.Service.Algorithms.Exceptions;
using Pathfinding.Service.Interface;

namespace Pathfinding.Service.Extensions;

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