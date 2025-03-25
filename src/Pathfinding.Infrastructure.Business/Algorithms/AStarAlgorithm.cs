using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Infrastructure.Business.Algorithms.StepRules;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public class AStarAlgorithm(IEnumerable<IPathfindingVertex> pathfindingRange,
    IStepRule stepRule, IHeuristic function) : DijkstraAlgorithm(pathfindingRange, stepRule)
{
    protected readonly Dictionary<Coordinate, double> accumulatedCosts = [];
    protected readonly Dictionary<Coordinate, double> heuristics = [];
    protected readonly IHeuristic heuristic = function;

    public AStarAlgorithm(IEnumerable<IPathfindingVertex> pathfindingRange)
        : this(pathfindingRange, new DefaultStepRule(), new ChebyshevDistance())
    {

    }

    protected override void DropState()
    {
        base.DropState();
        heuristics.Clear();
        accumulatedCosts.Clear();
    }

    protected override void PrepareForSubPathfinding(SubRange range)
    {
        base.PrepareForSubPathfinding(range);
        accumulatedCosts[CurrentRange.Source.Position] = default;
    }

    protected override void Enqueue(IPathfindingVertex vertex, double value)
    {
        if (!heuristics.TryGetValue(vertex.Position, out double cost))
        {
            cost = CalculateHeuristic(vertex);
            heuristics[vertex.Position] = cost;
        }
        base.Enqueue(vertex, value + cost);
        accumulatedCosts[vertex.Position] = value;
    }

    protected override double GetVertexCurrentCost(IPathfindingVertex vertex)
    {
        return accumulatedCosts.TryGetValue(vertex.Position, out double cost)
            ? cost
            : double.PositiveInfinity;
    }

    protected virtual double CalculateHeuristic(IPathfindingVertex vertex)
    {
        return heuristic.Calculate(vertex, CurrentRange.Target);
    }
}