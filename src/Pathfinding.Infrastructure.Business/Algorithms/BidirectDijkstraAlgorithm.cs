using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Infrastructure.Business.Algorithms.StepRules;
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;
using Priority_Queue;
using System.Collections.Frozen;

namespace Pathfinding.Infrastructure.Business.Algorithms
{
    public class BidirectDijkstraAlgorithm(IEnumerable<IPathfindingVertex> pathfindingRange,
        IStepRule stepRule) : BidirectWaveAlgorithm<SimplePriorityQueue<IPathfindingVertex, double>>(pathfindingRange)
    {
        protected readonly IStepRule stepRule = stepRule;

        public BidirectDijkstraAlgorithm(IEnumerable<IPathfindingVertex> pathfindingRange)
            : this(pathfindingRange, new DefaultStepRule())
        {

        }

        protected override void PrepareForSubPathfinding(SubRange range)
        {
            base.PrepareForSubPathfinding(range);
            forwardStorage.EnqueueOrUpdatePriority(Range.Source, default);
            backwardStorage.EnqueueOrUpdatePriority(Range.Target, default);
        }

        protected override IGraphPath GetSubPath()
        {
            return new BidirectGraphPath(
                forwardTraces.ToFrozenDictionary(),
                backwardTraces.ToFrozenDictionary(), Intersection, stepRule);
        }

        protected override void DropState()
        {
            base.DropState();
            forwardStorage.Clear();
            backwardStorage.Clear();
        }

        protected override void MoveNextVertex()
        {
            var forward = forwardStorage.TryFirstOrThrowDeadEndVertexException();
            var backward = backwardStorage.TryFirstOrThrowDeadEndVertexException();
            Current = new(forward, backward);
        }

        protected override void RelaxForwardVertex(IPathfindingVertex vertex)
        {
            double relaxedCost = GetForwardVertexRelaxedCost(vertex);
            double vertexCost = GetForwardVertexCurrentCost(vertex);
            if (vertexCost > relaxedCost)
            {
                EnqueueForward(vertex, relaxedCost);
                if (Intersection == NullPathfindingVertex.Instance
                    && backwardVisited.Contains(vertex))
                {
                    Intersection = vertex;
                }
                forwardTraces[vertex.Position] = Current.Source;
            }
        }

        protected override void RelaxBackwardVertex(IPathfindingVertex vertex)
        {
            double relaxedCost = GetBackwardVertexRelaxedCost(vertex);
            double vertexCost = GetBackwardVertexCurrentCost(vertex);
            if (vertexCost > relaxedCost)
            {
                EnqueueBackward(vertex, relaxedCost);
                if (Intersection == NullPathfindingVertex.Instance
                    && forwardVisited.Contains(vertex))
                {
                    Intersection = vertex;
                }
                backwardTraces[vertex.Position] = Current.Target;
            }
        }

        protected virtual void EnqueueForward(IPathfindingVertex vertex, double value)
        {
            forwardStorage.EnqueueOrUpdatePriority(vertex, value);
        }

        protected virtual void EnqueueBackward(IPathfindingVertex vertex, double value)
        {
            backwardStorage.EnqueueOrUpdatePriority(vertex, value);
        }

        protected virtual double GetForwardVertexCurrentCost(IPathfindingVertex vertex)
        {
            return forwardStorage.GetPriorityOrInfinity(vertex);
        }

        protected virtual double GetBackwardVertexCurrentCost(IPathfindingVertex vertex)
        {
            return backwardStorage.GetPriorityOrInfinity(vertex);
        }

        protected virtual double GetForwardVertexRelaxedCost(IPathfindingVertex neighbour)
        {
            return stepRule.CalculateStepCost(neighbour, Current.Source)
                   + GetForwardVertexCurrentCost(Current.Source);
        }

        protected virtual double GetBackwardVertexRelaxedCost(IPathfindingVertex neighbour)
        {
            return stepRule.CalculateStepCost(neighbour, Current.Target)
                   + GetBackwardVertexCurrentCost(Current.Target);
        }

        protected override void RelaxForwardNeighbours(IReadOnlyCollection<IPathfindingVertex> neighbours)
        {
            base.RelaxForwardNeighbours(neighbours);
            forwardStorage.TryRemove(Current.Source);
        }

        protected override void RelaxBackwardNeighbours(IReadOnlyCollection<IPathfindingVertex> vertices)
        {
            base.RelaxBackwardNeighbours(vertices);
            backwardStorage.TryRemove(Current.Target);
        }
    }
}
