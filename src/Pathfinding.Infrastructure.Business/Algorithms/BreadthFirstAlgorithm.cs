using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public abstract class BreadthFirstAlgorithm<TStorage>(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
    : WaveAlgorithm<TStorage>(pathfindingRange)
    where TStorage : new()
{
    protected override void RelaxVertex(IPathfindingVertex vertex)
    {
        Visited.Add(vertex);
        Traces[vertex.Position] = CurrentVertex;
    }
}
