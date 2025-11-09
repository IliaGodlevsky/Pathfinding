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
    private static readonly InclusiveValueRange<int> DefaultCostRange = new(1, 10);
    private static readonly int[,] CostMatrix = CreateCostMatrix();
    private static readonly bool[,] LinearAccessibility = CreateAccessibilityMap(GetLinearPathCoordinates());
    private static readonly bool[,] BranchAccessibility = CreateAccessibilityMap(
        GetLinearPathCoordinates().Concat(GetAlternativeBranchCoordinates()));

    public static TestGraph CreateLinearGraph()
    {
        var graph = AssembleGraph(new MatrixLayer(CostMatrix, LinearAccessibility));
        return CreateTestGraph(graph);
    }

    public static TestGraph CreateBranchingGraph()
    {
        var graph = AssembleGraph(new MatrixLayer(CostMatrix, BranchAccessibility));
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

    private static IReadOnlyList<Coordinate> GetAlternativeBranchCoordinates()
    {
        var coordinates = new List<Coordinate>(GridSize * 2 - 1);

        for (int y = 0; y < GridSize; y++)
        {
            coordinates.Add(new Coordinate(0, y));
        }

        for (int x = 1; x < GridSize; x++)
        {
            coordinates.Add(new Coordinate(x, GridSize - 1));
        }

        return coordinates;
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

    private static bool[,] CreateAccessibilityMap(IEnumerable<Coordinate> allowedCoordinates)
    {
        var accessibility = new bool[GridSize, GridSize];
        foreach (var coordinate in allowedCoordinates)
        {
            accessibility[coordinate[0], coordinate[1]] = true;
        }

        return accessibility;
    }

    private static int[,] CreateCostMatrix()
    {
        var matrix = new int[GridSize, GridSize];

        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                matrix[x, y] = 1;
            }
        }

        foreach (var coordinate in GetAlternativeBranchCoordinates())
        {
            if (!coordinate.Equals(StartCoordinate) && !coordinate.Equals(TargetCoordinate))
            {
                matrix[coordinate[0], coordinate[1]] = 5;
            }
        }

        return matrix;
    }

    private sealed class MatrixLayer(int[,] costs, bool[,] accessibility) : ILayer
    {
        public void Overlay(IGraph<IVertex> graph)
        {
            foreach (var vertex in graph)
            {
                var x = vertex.Position[0];
                var y = vertex.Position[1];

                vertex.IsObstacle = !accessibility[x, y];
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
