using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

internal sealed class TestPathfindingVertex(Coordinate position, int cost = 1, bool isObstacle = false) : IPathfindingVertex
{
    private readonly List<IPathfindingVertex> neighbors = [];

    public bool IsObstacle { get; set; } = isObstacle;

    public Coordinate Position { get; } = position;

    public IVertexCost Cost { get; } = new VertexCost(cost, (1, 10));

    public IReadOnlyCollection<IPathfindingVertex> Neighbors => neighbors;

    public void ConnectTo(TestPathfindingVertex vertex)
    {
        if (!neighbors.Contains(vertex))
        {
            neighbors.Add(vertex);
        }
    }

    public void ConnectBidirectional(TestPathfindingVertex vertex)
    {
        ConnectTo(vertex);
        vertex.ConnectTo(this);
    }

    public override string ToString()
    {
        return $"Vertex[{Position}]";
    }
}
