using Pathfinding.Infrastructure.Business.Algorithms.Events;
using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public abstract class PathfindingProcess(IReadOnlyCollection<IPathfindingVertex> range) 
    : IAlgorithm<IGraphPath>
{
    protected readonly record struct SubRange(
        IPathfindingVertex Source,
        IPathfindingVertex Target)
    {
        public static readonly SubRange Default = new(
            NullPathfindingVertex.Instance, 
            NullPathfindingVertex.Instance);
    }

    public event VertexProcessedEventHandler VertexProcessed;
    public event SubPathFoundEventHandler SubPathFound;

    public IGraphPath FindPath()
    {
        var subPaths = new List<IGraphPath>();
        foreach (var subRange in GetSubRanges())
        {
            PrepareForSubPathfinding(subRange);
            while (!IsDestination())
            {
                InspectCurrentVertex();
                MoveNextVertex();
                VisitCurrentVertex();
            }
            var subPath = GetSubPath();
            subPaths.Add(subPath);
            RaiseSubPathFound(subPath);
            DropState();
        }
        return CreatePath(subPaths);
    }

    protected abstract void MoveNextVertex();

    protected abstract void InspectCurrentVertex();

    protected abstract void VisitCurrentVertex();

    protected abstract bool IsDestination();

    protected abstract void DropState();

    protected abstract void PrepareForSubPathfinding(SubRange range);

    protected abstract IGraphPath GetSubPath();

    protected void RaiseVertexProcessed(IPathfindingVertex vertex,
        IEnumerable<IPathfindingVertex> vertices)
    {
        VertexProcessed?.Invoke(new(vertex, vertices));
    }

    protected void RaiseSubPathFound(IGraphPath subPath)
    {
        SubPathFound?.Invoke(new(subPath));
    }

    private IEnumerable<SubRange> GetSubRanges()
    {
        return range.Zip(range.Skip(1), (s, t) => new SubRange(s, t));
    }

    private static IGraphPath CreatePath(List<IGraphPath> subPaths)
    {
        return subPaths.Count switch
        {
            1 => subPaths[0],
            > 1 => new CompositeGraphPath(subPaths),
            _ => NullGraphPath.Instance
        };
    }
}