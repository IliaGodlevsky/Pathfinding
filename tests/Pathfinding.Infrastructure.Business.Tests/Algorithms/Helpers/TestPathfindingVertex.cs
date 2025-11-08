using System.Collections.Generic;
using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

internal sealed class TestPathfindingVertex : IPathfindingVertex
{
    private readonly List<IPathfindingVertex> neighbors = new();

    public TestPathfindingVertex(Coordinate position, int cost = 1, bool isObstacle = false)
    {
        Position = position;
        Cost = new VertexCost(cost, (1, 10));
        IsObstacle = isObstacle;
    }

    public bool IsObstacle { get; set; }

    public Coordinate Position { get; }

    public IVertexCost Cost { get; }

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
