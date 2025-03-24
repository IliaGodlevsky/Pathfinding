using Pathfinding.Infrastructure.Business.Algorithms.StepRules;
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Infrastructure.Data.Extensions;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;
using System.Collections;
using System.Collections.ObjectModel;

namespace Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;

using Traces = IReadOnlyDictionary<Coordinate, IPathfindingVertex>;

public sealed class GraphPath : IGraphPath
{
    private readonly Traces traces;
    private readonly IPathfindingVertex target;
    private readonly IStepRule stepRule;
    private readonly Lazy<IReadOnlyList<IPathfindingVertex>> path;
    private readonly Lazy<double> cost;
    private readonly Lazy<int> count;

    private IReadOnlyList<IPathfindingVertex> Path => path.Value;

    public double Cost => cost.Value;

    public int Count => count.Value;

    public GraphPath(Traces traces, IPathfindingVertex target)
        : this(traces, target, new DefaultStepRule())
    {

    }

    public GraphPath(Traces traces, IPathfindingVertex target, IStepRule stepRule)
    {
        this.traces = traces;
        this.target = target;
        this.stepRule = stepRule;
        path = new(GetPath);
        cost = new(GetPathCost);
        count = new(GetCount);
    }

    private ReadOnlyCollection<IPathfindingVertex> GetPath()
    {
        var vertices = new List<IPathfindingVertex>();
        var vertex = target;
        vertices.Add(vertex);
        var parent = traces.GetOrNullVertex(vertex.Position);
        while (parent.IsNeighbor(vertex))
        {
            vertices.Add(parent);
            vertex = parent;
            parent = traces.GetOrNullVertex(vertex.Position);
        }
        return vertices.AsReadOnly();
    }

    private double GetPathCost()
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
        for (int i = 0; i < Path.Count - 1; i++)
        {
            yield return Path[i].Position;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}