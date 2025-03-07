using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Infrastructure.Business.Algorithms.StepRules;
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Service.Interface;
using Priority_Queue;
using System.Collections.Frozen;

namespace Pathfinding.Infrastructure.Business.Algorithms
{
    public class DijkstraAlgorithm(IEnumerable<IPathfindingVertex> pathfindingRange, IStepRule stepRule)
        : WaveAlgorithm<SimplePriorityQueue<IPathfindingVertex, double>>(pathfindingRange)
    {
        protected readonly IStepRule stepRule = stepRule;

        public DijkstraAlgorithm(IEnumerable<IPathfindingVertex> pathfindingRange)
            : this(pathfindingRange, new DefaultStepRule())
        {

        }

        protected override IGraphPath GetSubPath()
        {
            return new GraphPath(traces.ToFrozenDictionary(),
                CurrentRange.Target, stepRule);
        }

        protected override void DropState()
        {
            base.DropState();
            storage.Clear();
        }

        protected override void MoveNextVertex()
        {
            CurrentVertex = storage.TryFirstOrThrowDeadEndVertexException();
        }

        protected override void PrepareForSubPathfinding(SubRange range)
        {
            base.PrepareForSubPathfinding(range);
            storage.EnqueueOrUpdatePriority(CurrentRange.Source, default);
        }

        protected override void RelaxVertex(IPathfindingVertex vertex)
        {
            double relaxedCost = GetVertexRelaxedCost(vertex);
            double vertexCost = GetVertexCurrentCost(vertex);
            if (vertexCost > relaxedCost)
            {
                Enqueue(vertex, relaxedCost);
                traces[vertex.Position] = CurrentVertex;
            }
        }

        protected virtual void Enqueue(IPathfindingVertex vertex, double value)
        {
            storage.EnqueueOrUpdatePriority(vertex, value);
        }

        protected virtual double GetVertexCurrentCost(IPathfindingVertex vertex)
        {
            return storage.GetPriorityOrInfinity(vertex);
        }

        protected virtual double GetVertexRelaxedCost(IPathfindingVertex neighbour)
        {
            return stepRule.CalculateStepCost(neighbour, CurrentVertex)
                   + GetVertexCurrentCost(CurrentVertex);
        }

        protected override void RelaxNeighbours(IReadOnlyCollection<IPathfindingVertex> neighbours)
        {
            base.RelaxNeighbours(neighbours);
            storage.TryRemove(CurrentVertex);
        }
    }
}