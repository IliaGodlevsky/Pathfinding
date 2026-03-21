using Pathfinding.Infrastructure.Business.Algorithms.StepRules;
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Infrastructure.Data.Extensions;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;
using System.Collections;
using System.Collections.ObjectModel;

namespace Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;

public sealed class BidirectGraphPath : IGraphPath
{
    private readonly IReadOnlyDictionary<Coordinate, IPathfindingVertex> forwardTraces;
    private readonly IReadOnlyDictionary<Coordinate, IPathfindingVertex> backwardTraces;
    private readonly IPathfindingVertex intersection;
    private readonly IStepRule stepRule;

    private readonly Lazy<IReadOnlyList<IPathfindingVertex>> path;
    private readonly Lazy<double> cost;
    private readonly Lazy<int> count;

    private IReadOnlyList<IPathfindingVertex> Path => path.Value;

    public double Cost => cost.Value;

    public int Count => count.Value;

    public BidirectGraphPath(
        IReadOnlyDictionary<Coordinate, IPathfindingVertex> forwardTraces,
        IReadOnlyDictionary<Coordinate, IPathfindingVertex> backwardTraces,
        IPathfindingVertex intersection,
        IStepRule stepRule)
    {
        this.forwardTraces = forwardTraces;
        this.backwardTraces = backwardTraces;
        this.intersection = intersection;
        this.stepRule = stepRule;
        path = new(GetPath);
        cost = new(GetCost);
        count = new(GetCount);
    }

    public BidirectGraphPath(
        IReadOnlyDictionary<Coordinate, IPathfindingVertex> forwardTraces,
        IReadOnlyDictionary<Coordinate, IPathfindingVertex> backwardTraces,
        IPathfindingVertex intersection)
        : this(forwardTraces, backwardTraces, intersection, new DefaultStepRule())
    {

    }

    private List<IPathfindingVertex> UnrollPath(
        IReadOnlyDictionary<Coordinate, IPathfindingVertex> traces,
        bool isBackward = false)
    {
        IPathfindingVertex[] initial = isBackward ? [] : [intersection];
        var vertices = new List<IPathfindingVertex>(initial);
        var vertex = intersection;
        var parent = traces.GetOrNullVertex(vertex.Position);
        while (parent.IsNeighbor(vertex))
        {
            vertices.Add(parent);
            vertex = parent;
            parent = traces.GetOrNullVertex(vertex.Position);
        }
        if (isBackward)
        {
            vertices.Reverse();
        }
        return vertices;
    }

    private ReadOnlyCollection<IPathfindingVertex> GetPath()
    {
        var forward = UnrollPath(forwardTraces);
        var backward = UnrollPath(backwardTraces, true);
        return Array.AsReadOnly([.. backward, .. forward]);
    }

    private double GetCost()
    {
        double totalCost = 0;
        for (int i = 0; i < Count; i++)
        {
            totalCost += stepRule.CalculateStepCost(Path[i], Path[i + 1]);
        }
        return totalCost;
    }

    private int GetCount()
    {
        return Path.Count == 0 ? 0 : Path.Count - 1;
    }

    public IEnumerator<Coordinate> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return Path[i].Position;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
