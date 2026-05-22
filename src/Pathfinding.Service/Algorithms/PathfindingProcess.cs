using Pathfinding.Data;
using Pathfinding.Service.Algorithms.Events;
using Pathfinding.Service.Algorithms.GraphPaths;
using Pathfinding.Service.Interface;

namespace Pathfinding.Service.Algorithms;

public abstract class PathfindingProcess(IReadOnlyCollection<IPathfindingVertex> range)
    : IPathfindingAlgorithm<IGraphPath>
{
    protected readonly record struct SubRange(
        IPathfindingVertex Source,
        IPathfindingVertex Target)
    {
        public static SubRange From(IPathfindingVertex source, 
            IPathfindingVertex target) => new(source, target);

        public static readonly SubRange Default = new(
            NullPathfindingVertex.Instance,
            NullPathfindingVertex.Instance);
    }

    public event VertexProcessedEventHandler VertexProcessed;
    public event SubPathFoundEventHandler SubPathFound;

    public IGraphPath FindPath()
    {
        var subPaths = new List<IGraphPath>();
        var subRanges = range
            .Zip(range.Skip(1), SubRange.From)
            .ToArray();
        foreach (var subRange in subRanges)
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
            SubPathFound?.Invoke(new(subPath));
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