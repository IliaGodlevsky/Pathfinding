using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public abstract class BidirectBreadthFirstAlgorithm<TStorage>(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
    : BidirectWaveAlgorithm<TStorage>(pathfindingRange)
    where TStorage : new()
{
    protected override void RelaxForwardVertex(IPathfindingVertex vertex)
    {
        ForwardVisited.Add(vertex);
        ForwardTraces[vertex.Position] = Current.Source;
    }

    protected override void RelaxBackwardVertex(IPathfindingVertex vertex)
    {
        BackwardVisited.Add(vertex);
        BackwardTraces[vertex.Position] = Current.Target;
    }
}
