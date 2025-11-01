using Pathfinding.Infrastructure.Business.Algorithms.Exceptions;
using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public sealed class BeamSearchAlgorithm(
    IReadOnlyCollection<IPathfindingVertex> pathfindingRange,
    IHeuristic heuristic,
    double beamWidthPercentage = DefaultBeamWidthPercentage)
    : WaveAlgorithm<List<BeamSearchAlgorithm.BeamEntry>>(pathfindingRange)
{
    public const double DefaultBeamWidthPercentage = 10;

    private readonly IHeuristic heuristicFunction = heuristic ?? throw new ArgumentNullException(nameof(heuristic));
    private readonly double beamWidthFraction = ValidateBeamWidth(beamWidthPercentage);

    private readonly Dictionary<Coordinate, double> frontierPriorities = [];

    public BeamSearchAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
        : this(pathfindingRange, new ManhattanDistance())
    {
    }

    protected override void PrepareForSubPathfinding(SubRange range)
    {
        base.PrepareForSubPathfinding(range);
        Storage.Clear();
        frontierPriorities.Clear();
    }

    protected override void DropState()
    {
        base.DropState();
        Storage.Clear();
        frontierPriorities.Clear();
    }

    protected override void MoveNextVertex()
    {
        if (Storage.Count == 0)
        {
            throw new DeadendVertexException();
        }

        var next = Storage[0];
        Storage.RemoveAt(0);
        frontierPriorities.Remove(next.Vertex.Position);
        CurrentVertex = next.Vertex;
    }

    protected override void RelaxVertex(IPathfindingVertex vertex)
    {
        if (Visited.Contains(vertex))
        {
            return;
        }

        var priority = CalculatePriority(vertex);
        if (frontierPriorities.TryGetValue(vertex.Position, out var current)
            && current <= priority)
        {
            return;
        }

        if (frontierPriorities.ContainsKey(vertex.Position))
        {
            RemoveVertex(vertex);
        }

        Storage.Add(new(vertex, priority));
        frontierPriorities[vertex.Position] = priority;
        Storage.Sort(CompareEntries);
        TrimBeam();
        Traces[vertex.Position] = CurrentVertex;
    }

    private double CalculatePriority(IPathfindingVertex vertex)
    {
        return heuristicFunction.Calculate(vertex, CurrentRange.Target);
    }

    private void TrimBeam()
    {
        if (Storage.Count == 0)
        {
            return;
        }

        int beamWidthLimit = CalculateBeamWidthLimit();
        while (Storage.Count > beamWidthLimit)
        {
            var removed = Storage[^1];
            frontierPriorities.Remove(removed.Vertex.Position);
            Traces.Remove(removed.Vertex.Position);
            Storage.RemoveAt(Storage.Count - 1);
        }
    }

    private int CalculateBeamWidthLimit()
    {
        int limit = (int)Math.Ceiling(Storage.Count * beamWidthFraction);
        return Math.Clamp(limit, 1, Storage.Count);
    }

    private static double ValidateBeamWidth(double beamWidthPercentage)
    {
        if (double.IsNaN(beamWidthPercentage) || double.IsInfinity(beamWidthPercentage))
        {
            throw new ArgumentOutOfRangeException(nameof(beamWidthPercentage));
        }

        if (beamWidthPercentage <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(beamWidthPercentage));
        }

        double clampedPercentage = Math.Min(beamWidthPercentage, 100);
        return clampedPercentage / 100d;
    }

    private void RemoveVertex(IPathfindingVertex vertex)
    {
        for (int i = 0; i < Storage.Count; i++)
        {
            if (Storage[i].Vertex.Equals(vertex))
            {
                Storage.RemoveAt(i);
                break;
            }
        }
    }

    private static int CompareEntries(BeamEntry left, BeamEntry right)
    {
        return left.Priority.CompareTo(right.Priority);
    }

    public readonly record struct BeamEntry(IPathfindingVertex Vertex, double Priority);
}
