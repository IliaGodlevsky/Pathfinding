using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;
using Priority_Queue;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public sealed class AStarLeeAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange,
    IHeuristic function) : BreadthFirstAlgorithm<SimplePriorityQueue<IPathfindingVertex, double>>(pathfindingRange)
{
    private readonly Dictionary<Coordinate, double> heuristics = [];

    public AStarLeeAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
        : this(pathfindingRange, new ManhattanDistance())
    {

    }

    protected override void MoveNextVertex()
    {
        CurrentVertex = Storage.TryFirstOrThrowDeadEndVertexException();
    }

    protected override void DropState()
    {
        base.DropState();
        Storage.Clear();
        heuristics.Clear();
    }

    protected override void PrepareForSubPathfinding(SubRange range)
    {
        base.PrepareForSubPathfinding(range);
        var value = CalculateHeuristic(CurrentRange.Source);
        heuristics[CurrentRange.Source.Position] = value;
    }

    protected override void RelaxVertex(IPathfindingVertex vertex)
    {
        if (!heuristics.TryGetValue(vertex.Position, out var cost))
        {
            cost = CalculateHeuristic(vertex);
            heuristics[vertex.Position] = cost;
        }
        Storage.Enqueue(vertex, cost);
        base.RelaxVertex(vertex);
    }

    private double CalculateHeuristic(IPathfindingVertex vertex)
    {
        return function.Calculate(vertex, CurrentRange.Target);
    }

    protected override void RelaxNeighbours(IReadOnlyCollection<IPathfindingVertex> neighbours)
    {
        base.RelaxNeighbours(neighbours);
        Storage.TryRemove(CurrentVertex);
    }
}