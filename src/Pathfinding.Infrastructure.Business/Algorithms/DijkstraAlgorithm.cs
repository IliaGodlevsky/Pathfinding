using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Infrastructure.Business.Algorithms.StepRules;
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Service.Interface;
using Priority_Queue;
using System.Collections.Frozen;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public class DijkstraAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange, IStepRule stepRule)
    : WaveAlgorithm<SimplePriorityQueue<IPathfindingVertex, double>>(pathfindingRange)
{
    protected readonly IStepRule StepRule = stepRule;

    public DijkstraAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
        : this(pathfindingRange, new DefaultStepRule())
    {

    }

    protected override GraphPath GetSubPath()
    {
        return new(
            Traces.ToFrozenDictionary(),
            CurrentRange.Target,
            StepRule);
    }

    protected override void DropState()
    {
        base.DropState();
        Storage.Clear();
    }

    protected override void MoveNextVertex()
    {
        CurrentVertex = Storage.TryFirstOrThrowDeadEndVertexException();
    }

    protected override void PrepareForSubPathfinding(SubRange range)
    {
        base.PrepareForSubPathfinding(range);
        Storage.EnqueueOrUpdatePriority(CurrentRange.Source, 0);
    }

    protected override void RelaxVertex(IPathfindingVertex vertex)
    {
        var relaxedCost = GetVertexRelaxedCost(vertex);
        var vertexCost = GetVertexCurrentCost(vertex);
        if (vertexCost > relaxedCost)
        {
            Enqueue(vertex, relaxedCost);
            Traces[vertex.Position] = CurrentVertex;
        }
    }

    protected virtual void Enqueue(IPathfindingVertex vertex, double value)
    {
        Storage.EnqueueOrUpdatePriority(vertex, value);
    }

    protected virtual double GetVertexCurrentCost(IPathfindingVertex vertex)
    {
        return Storage.GetPriorityOrInfinity(vertex);
    }

    protected virtual double GetVertexRelaxedCost(IPathfindingVertex neighbour)
    {
        return StepRule.CalculateStepCost(neighbour, CurrentVertex)
               + GetVertexCurrentCost(CurrentVertex);
    }

    protected override void RelaxNeighbours(IReadOnlyCollection<IPathfindingVertex> neighbours)
    {
        base.RelaxNeighbours(neighbours);
        Storage.TryRemove(CurrentVertex);
    }
}