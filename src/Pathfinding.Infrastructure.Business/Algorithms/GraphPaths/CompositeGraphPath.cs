using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;
using System.Collections;

namespace Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;

public sealed class CompositeGraphPath : IGraphPath
{
    private readonly IReadOnlyCollection<IGraphPath> paths;
    private readonly Lazy<IReadOnlyCollection<Coordinate>> path;
    private readonly Lazy<int> count;
    private readonly Lazy<double> cost;

    private IReadOnlyCollection<Coordinate> Path => path.Value;

    public int Count => count.Value;

    public double Cost => cost.Value;

    public CompositeGraphPath(IReadOnlyCollection<IGraphPath> paths)
    {
        this.paths = paths;
        path = new(GetPath);
        count = new(GetCount);
        cost = new(GetCost);
    }

    private Coordinate[] GetPath()
    {
        return [.. paths.SelectMany(p => p.Reverse())];
    }

    private int GetCount()
    {
        return Path.Count == 0 ? 0 : paths.Sum(p => p.Count);
    }

    private double GetCost()
    {
        return Path.Count == 0 ? 0 : paths.Sum(p => p.Cost);
    }

    public IEnumerator<Coordinate> GetEnumerator()
    {
        return Path.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}