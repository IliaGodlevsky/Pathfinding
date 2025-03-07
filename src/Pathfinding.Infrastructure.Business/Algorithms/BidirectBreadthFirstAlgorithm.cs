using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms
{
    public abstract class BidirectBreadthFirstAlgorithm<TStorage>(IEnumerable<IPathfindingVertex> pathfindingRange) 
        : BidirectWaveAlgorithm<TStorage>(pathfindingRange)
        where TStorage : new()
    {
        protected override void RelaxForwardVertex(IPathfindingVertex vertex)
        {
            forwardVisited.Add(vertex);
            forwardTraces[vertex.Position] = Current.Source;
        }

        protected override void RelaxBackwardVertex(IPathfindingVertex vertex)
        {
            backwardVisited.Add(vertex);
            backwardTraces[vertex.Position] = Current.Target;
        }
    }
}
