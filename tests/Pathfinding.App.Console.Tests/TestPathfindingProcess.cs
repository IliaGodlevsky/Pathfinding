using Pathfinding.App.Console.Factories;
using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.App.Console.Tests;

internal sealed class TestAlgorithmFactory : IAlgorithmFactory<PathfindingProcess>
{
    public PathfindingProcess CreateAlgorithm(
        IReadOnlyCollection<IPathfindingVertex> range,
        IAlgorithmBuildInfo info)
    {
        return new TestPathfindingProcess(range);
    }
}

internal sealed class TestPathfindingProcess : PathfindingProcess
{
    private SubRange currentRange;
    private bool isDestination;

    public TestPathfindingProcess(IReadOnlyCollection<IPathfindingVertex> range)
        : base(range)
    {
    }

    protected override void PrepareForSubPathfinding(SubRange range)
    {
        currentRange = range;
        isDestination = false;
    }

    protected override bool IsDestination() => isDestination;

    protected override void MoveNextVertex()
    {
        isDestination = true;
    }

    protected override void InspectCurrentVertex()
    {
    }

    protected override void VisitCurrentVertex()
    {
        if (currentRange.Source != NullPathfindingVertex.Interface)
        {
            var neighbors = currentRange.Target == NullPathfindingVertex.Interface
                ? Array.Empty<IPathfindingVertex>()
                : new[] { currentRange.Target };
            RaiseVertexProcessed(currentRange.Source, neighbors);
        }
    }

    protected override IGraphPath GetSubPath() => NullGraphPath.Interface;

    protected override void DropState()
    {
    }
}
