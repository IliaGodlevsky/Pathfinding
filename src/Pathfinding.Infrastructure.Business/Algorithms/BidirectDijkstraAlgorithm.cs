using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Infrastructure.Business.Algorithms.StepRules;
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Service.Interface;
using Priority_Queue;
using System.Collections.Frozen;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public class BidirectDijkstraAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange,
    IStepRule stepRule) : BidirectWaveAlgorithm<SimplePriorityQueue<IPathfindingVertex, double>>(pathfindingRange)
{
    protected readonly IStepRule StepRule = stepRule;

    public BidirectDijkstraAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
        : this(pathfindingRange, new DefaultStepRule())
    {

    }

    protected override void PrepareForSubPathfinding(SubRange range)
    {
        base.PrepareForSubPathfinding(range);
        ForwardStorage.EnqueueOrUpdatePriority(Range.Source, 0);
        BackwardStorage.EnqueueOrUpdatePriority(Range.Target, 0);
    }

    protected override BidirectGraphPath GetSubPath()
    {
        return new (
            ForwardTraces.ToFrozenDictionary(),
            BackwardTraces.ToFrozenDictionary(), 
            Intersection,
            StepRule);
    }

    protected override void DropState()
    {
        base.DropState();
        ForwardStorage.Clear();
        BackwardStorage.Clear();
    }

    protected override void MoveNextVertex()
    {
        var forward = ForwardStorage.TryFirstOrThrowDeadEndVertexException();
        var backward = BackwardStorage.TryFirstOrThrowDeadEndVertexException();
        Current = new(forward, backward);
    }

    protected override void RelaxForwardVertex(IPathfindingVertex vertex)
    {
        var relaxedCost = GetForwardVertexRelaxedCost(vertex);
        var vertexCost = GetForwardVertexCurrentCost(vertex);
        if (vertexCost > relaxedCost)
        {
            EnqueueForward(vertex, relaxedCost);
            SetForwardIntersection(vertex);
            ForwardTraces[vertex.Position] = Current.Source;
        }
    }

    protected override void RelaxBackwardVertex(IPathfindingVertex vertex)
    {
        var relaxedCost = GetBackwardVertexRelaxedCost(vertex);
        var vertexCost = GetBackwardVertexCurrentCost(vertex);
        if (vertexCost > relaxedCost)
        {
            EnqueueBackward(vertex, relaxedCost);
            SetBackwardIntersections(vertex);
            BackwardTraces[vertex.Position] = Current.Target;
        }
    }

    protected virtual void EnqueueForward(IPathfindingVertex vertex, double value)
    {
        ForwardStorage.EnqueueOrUpdatePriority(vertex, value);
    }

    protected virtual void EnqueueBackward(IPathfindingVertex vertex, double value)
    {
        BackwardStorage.EnqueueOrUpdatePriority(vertex, value);
    }

    protected virtual double GetForwardVertexCurrentCost(IPathfindingVertex vertex)
    {
        return ForwardStorage.GetPriorityOrInfinity(vertex);
    }

    protected virtual double GetBackwardVertexCurrentCost(IPathfindingVertex vertex)
    {
        return BackwardStorage.GetPriorityOrInfinity(vertex);
    }

    protected virtual double GetForwardVertexRelaxedCost(IPathfindingVertex neighbour)
    {
        return StepRule.CalculateStepCost(neighbour, Current.Source)
               + GetForwardVertexCurrentCost(Current.Source);
    }

    protected virtual double GetBackwardVertexRelaxedCost(IPathfindingVertex neighbour)
    {
        return StepRule.CalculateStepCost(neighbour, Current.Target)
               + GetBackwardVertexCurrentCost(Current.Target);
    }

    protected override void RelaxForwardNeighbours(IReadOnlyCollection<IPathfindingVertex> neighbours)
    {
        base.RelaxForwardNeighbours(neighbours);
        ForwardStorage.TryRemove(Current.Source);
    }

    protected override void RelaxBackwardNeighbours(IReadOnlyCollection<IPathfindingVertex> vertices)
    {
        base.RelaxBackwardNeighbours(vertices);
        BackwardStorage.TryRemove(Current.Target);
    }
}
