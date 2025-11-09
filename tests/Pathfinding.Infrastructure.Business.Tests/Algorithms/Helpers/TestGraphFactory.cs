using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Business.Layers;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Shared.Primitives;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

internal static class TestGraphFactory
{
    private const int GridSize = 10;

    private static readonly Coordinate StartCoordinate = new(0, 0);
    private static readonly Coordinate TargetCoordinate = new(GridSize - 1, GridSize - 1);
    private static readonly InclusiveValueRange<int> DefaultCostRange = new(1, 9);

    private static readonly int[,] CostMatrix = new int[GridSize, GridSize]
    {
        { 1, 1, 2, 2, 3, 3, 2, 2, 1, 1 },
        { 8, 4, 5, 6, 5, 6, 5, 6, 4, 2 },
        { 9, 5, 6, 7, 6, 7, 6, 7, 5, 2 },
        { 9, 6, 7, 8, 7, 8, 7, 8, 6, 2 },
        { 9, 7, 8, 9, 8, 9, 8, 9, 7, 3 },
        { 9, 8, 9, 8, 9, 8, 9, 8, 8, 3 },
        { 9, 9, 8, 7, 6, 7, 8, 9, 7, 2 },
        { 8, 8, 7, 6, 5, 6, 7, 8, 6, 2 },
        { 7, 7, 6, 5, 4, 5, 6, 7, 5, 2 },
        { 6, 6, 5, 4, 3, 4, 5, 6, 4, 2 },
    };

    private static readonly bool[,] LinearObstacles = new bool[GridSize, GridSize]
    {
        { false, false, false, false, false, false, false, false, false, false },
        { true,  true,  true,  true,  true,  true,  true,  true,  true,  false },
        { true,  true,  true,  true,  true,  true,  true,  true,  true,  false },
        { true,  true,  true,  true,  true,  true,  true,  true,  true,  false },
        { true,  true,  true,  true,  true,  true,  true,  true,  true,  false },
        { true,  true,  true,  true,  true,  true,  true,  true,  true,  false },
        { true,  true,  true,  true,  true,  true,  true,  true,  true,  false },
        { true,  true,  true,  true,  true,  true,  true,  true,  true,  false },
        { true,  true,  true,  true,  true,  true,  true,  true,  true,  false },
        { true,  true,  true,  true,  true,  true,  true,  true,  true,  false },
    };

    private static readonly bool[,] BranchObstacles = new bool[GridSize, GridSize]
    {
        { false, false, false, false, false, false, false, false, false, false },
        { false, true,  true,  true,  true,  true,  true,  true,  true,  false },
        { false, true,  true,  true,  true,  true,  true,  true,  true,  false },
        { false, true,  true,  true,  true,  true,  true,  true,  true,  false },
        { false, true,  true,  true,  true,  true,  true,  true,  true,  false },
        { false, true,  true,  true,  true,  true,  true,  true,  true,  false },
        { false, true,  true,  true,  true,  true,  true,  true,  true,  false },
        { false, true,  true,  true,  true,  true,  true,  true,  true,  false },
        { false, true,  true,  true,  true,  true,  true,  true,  true,  false },
        { false, false, false, false, false, false, false, false, false, false },
    };

    public static TestGraph CreateLinearGraph()
    {
        var graph = AssembleGraph(new MatrixLayer(CostMatrix, LinearObstacles));
        return CreateTestGraph(graph);
    }

    public static TestGraph CreateBranchingGraph()
    {
        var graph = AssembleGraph(new MatrixLayer(CostMatrix, BranchObstacles));
        return CreateTestGraph(graph);
    }

    internal static IReadOnlyList<Coordinate> GetLinearPathCoordinates()
    {
        var coordinates = new List<Coordinate>(GridSize * 2 - 1);

        for (int x = 0; x < GridSize; x++)
        {
            coordinates.Add(new Coordinate(x, 0));
        }

        for (int y = 1; y < GridSize; y++)
        {
            coordinates.Add(new Coordinate(GridSize - 1, y));
        }

        return coordinates;
    }

    internal static double GetLinearPathCost()
    {
        return CalculatePathCost(GetLinearPathCoordinates());
    }

    private static IGraph<TestVertex> AssembleGraph(params ILayer[] overlays)
    {
        var graph = new GraphAssemble<TestVertex>().AssembleGraph([GridSize, GridSize]);
        var neighborhoodLayer = new MooreNeighborhoodLayer();

        foreach (var overlay in overlays.Append(neighborhoodLayer))
        {
            overlay.Overlay((IGraph<IVertex>)graph);
        }

        return graph;
    }

    private static TestGraph CreateTestGraph(IGraph<TestVertex> graph)
    {
        var start = (IPathfindingVertex)graph.Get(StartCoordinate);
        var target = (IPathfindingVertex)graph.Get(TargetCoordinate);
        var vertices = graph
            .Where(vertex => !vertex.IsObstacle)
            .Cast<IPathfindingVertex>()
            .ToList();

        return new TestGraph(start, target, vertices);
    }

    private static double CalculatePathCost(IEnumerable<Coordinate> coordinates)
    {
        double totalCost = 0;

        foreach (var coordinate in coordinates.Skip(1))
        {
            totalCost += CostMatrix[coordinate[0], coordinate[1]];
        }

        return totalCost;
    }

    private sealed class MatrixLayer(int[,] costs, bool[,] obstacles) : ILayer
    {
        public void Overlay(IGraph<IVertex> graph)
        {
            foreach (var vertex in graph)
            {
                var x = vertex.Position[0];
                var y = vertex.Position[1];

                vertex.IsObstacle = obstacles[x, y];
                vertex.Cost = new VertexCost(costs[x, y], DefaultCostRange);
            }
        }
    }

    private sealed class TestVertex : IVertex, IPathfindingVertex
    {
        private TestVertex[] neighbours = Array.Empty<TestVertex>();

        public bool IsObstacle { get; set; }

        public IVertexCost Cost { get; set; } = new VertexCost(1, DefaultCostRange);

        public Coordinate Position { get; set; } = new Coordinate(0, 0);

        public IReadOnlyCollection<IVertex> Neighbors
        {
            get => neighbours;
            set => neighbours = value?.Cast<TestVertex>().ToArray() ?? Array.Empty<TestVertex>();
        }

        IReadOnlyCollection<IPathfindingVertex> IPathfindingVertex.Neighbors => neighbours;

        public bool Equals(IVertex other)
        {
            return other is not null && Position.Equals(other.Position);
        }

        public override bool Equals(object? obj)
        {
            return obj is IVertex vertex && Equals(vertex);
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
}

internal sealed record TestGraph(
    IPathfindingVertex Start,
    IPathfindingVertex Target,
    IReadOnlyCollection<IPathfindingVertex> Vertices)
{
    public IReadOnlyCollection<IPathfindingVertex> Range => [Start, Target];
}
