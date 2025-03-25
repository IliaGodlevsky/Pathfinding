using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public abstract class BreadthFirstAlgorithm<TStorage>(IEnumerable<IPathfindingVertex> pathfindingRange)
    : WaveAlgorithm<TStorage>(pathfindingRange)
    where TStorage : new()
{
    protected override void RelaxVertex(IPathfindingVertex vertex)
    {
        visited.Add(vertex);
        traces[vertex.Position] = CurrentVertex;
    }
}
