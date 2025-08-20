using Pathfinding.Infrastructure.Business.Algorithms.Exceptions;
using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Infrastructure.Business.Algorithms.StepRules;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;
using System.Collections.Frozen;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public sealed class IdaStarAlgorithm(
    IReadOnlyCollection<IPathfindingVertex> pathfindingRange,
    IStepRule stepRule, IHeuristic heuristic) 
    : PathfindingAlgorithm<Stack<IPathfindingVertex>>(pathfindingRange)
{
    private readonly Dictionary<Coordinate, double> gCosts = [];
    private double currentBound;
    private double nextBound;

    public IdaStarAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
        : this(pathfindingRange, new DefaultStepRule(), new ChebyshevDistance())
    {
    }

    protected override void DropState()
    {
        base.DropState();
        Storage.Clear();
        gCosts.Clear();
        currentBound = 0;
        nextBound = double.PositiveInfinity;
    }

    protected override GraphPath GetSubPath()
    {
        return new (
            Traces.ToFrozenDictionary(), 
            CurrentRange.Target,
            stepRule);
    }

    protected override void PrepareForSubPathfinding(SubRange range)
    {
        base.PrepareForSubPathfinding(range);
        
        var initialHeuristic = heuristic.Calculate(CurrentRange.Source,
            CurrentRange.Target);
        currentBound = initialHeuristic;
        nextBound = double.PositiveInfinity;
        
        gCosts[CurrentRange.Source.Position] = 0;
        Storage.Push(CurrentRange.Source);
        Traces[CurrentRange.Source.Position] = NullPathfindingVertex.Interface;
    }

    protected override void MoveNextVertex()
    {
        if (Storage.Count == 0)
        {
            if (nextBound == double.PositiveInfinity)
            {
                throw new DeadendVertexException("No path found");
            }

            currentBound = nextBound;
            nextBound = double.PositiveInfinity;
            
            Visited.Clear();
            Storage.Clear();
            gCosts.Clear();
            
            gCosts[CurrentRange.Source.Position] = 0;
            Storage.Push(CurrentRange.Source);
            CurrentVertex = CurrentRange.Source;
            return;
        }

        CurrentVertex = Storage.Pop();
    }

    protected override void VisitCurrentVertex()
    {
        Visited.Add(CurrentVertex);
    }

    protected override void InspectCurrentVertex()
    {
        var g = gCosts[CurrentVertex.Position];
        var f = g + heuristic.Calculate(CurrentVertex, CurrentRange.Target);
        
        if (f > currentBound)
        {
            nextBound = Math.Min(nextBound, f);
            return;
        }

        var neighbors = GetUnvisitedNeighbours(CurrentVertex);
        RaiseVertexProcessed(CurrentVertex, neighbors);
        
        foreach (var neighbor in neighbors.Reverse())
        {
            var newG = g + stepRule.CalculateStepCost(neighbor, CurrentVertex);
            
            if (!gCosts.TryGetValue(neighbor.Position, out var oldG) || newG < oldG)
            {
                gCosts[neighbor.Position] = newG;
                Storage.Push(neighbor);
                Traces[neighbor.Position] = CurrentVertex;
            }
        }
    }
}
