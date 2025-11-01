using Pathfinding.Infrastructure.Business.Algorithms.Exceptions;
using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public sealed class BeamSearchAlgorithm(
    IReadOnlyCollection<IPathfindingVertex> pathfindingRange,
    IHeuristic heuristic,
    int beamWidth = DefaultBeamWidth)
    : WaveAlgorithm<List<BeamSearchAlgorithm.BeamEntry>>(pathfindingRange)
{
    public const int DefaultBeamWidth = 10;

    private readonly IHeuristic heuristicFunction = heuristic ?? throw new ArgumentNullException(nameof(heuristic));
    private readonly int beamWidthLimit = beamWidth > 0
        ? beamWidth
        : throw new ArgumentOutOfRangeException(nameof(beamWidth));

    private readonly Dictionary<Coordinate, double> frontierPriorities = [];

    public BeamSearchAlgorithm(
        IReadOnlyCollection<IPathfindingVertex> pathfindingRange,
        int beamWidthLimit = DefaultBeamWidth)
        : this(pathfindingRange, new ManhattanDistance(), beamWidthLimit)
    {
    }

    public BeamSearchAlgorithm(
        IReadOnlyCollection<IPathfindingVertex> pathfindingRange,
        IHeuristic heuristic)
        : this(pathfindingRange, heuristic, DefaultBeamWidth)
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
        while (Storage.Count > beamWidthLimit)
        {
            var removed = Storage[^1];
            frontierPriorities.Remove(removed.Vertex.Position);
            Traces.Remove(removed.Vertex.Position);
            Storage.RemoveAt(Storage.Count - 1);
        }
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
