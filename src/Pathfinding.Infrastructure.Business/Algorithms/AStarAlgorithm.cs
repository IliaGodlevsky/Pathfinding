using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Infrastructure.Business.Algorithms.StepRules;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public class AStarAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange,
    IStepRule stepRule, IHeuristic function) : DijkstraAlgorithm(pathfindingRange, stepRule)
{
    protected readonly Dictionary<Coordinate, double> AccumulatedCosts = [];
    protected readonly Dictionary<Coordinate, double> Heuristics = [];
    protected readonly IHeuristic Heuristic = function;

    public AStarAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
        : this(pathfindingRange, new DefaultStepRule(), new ChebyshevDistance())
    {

    }

    protected override void DropState()
    {
        base.DropState();
        Heuristics.Clear();
        AccumulatedCosts.Clear();
    }

    protected override void PrepareForSubPathfinding(SubRange range)
    {
        base.PrepareForSubPathfinding(range);
        AccumulatedCosts[CurrentRange.Source.Position] = 0;
    }

    protected override void Enqueue(IPathfindingVertex vertex, double value)
    {
        if (!Heuristics.TryGetValue(vertex.Position, out var cost))
        {
            cost = CalculateHeuristic(vertex);
            Heuristics[vertex.Position] = cost;
        }
        base.Enqueue(vertex, value + cost);
        AccumulatedCosts[vertex.Position] = value;
    }

    protected override double GetVertexCurrentCost(IPathfindingVertex vertex)
    {
        return AccumulatedCosts.GetValueOrDefault(vertex.Position, double.PositiveInfinity);
    }

    protected virtual double CalculateHeuristic(IPathfindingVertex vertex)
    {
        return Heuristic.Calculate(vertex, CurrentRange.Target);
    }
}