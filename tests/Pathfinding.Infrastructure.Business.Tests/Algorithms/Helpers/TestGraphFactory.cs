using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Business.Layers;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

internal static class TestGraphFactory
{
    private const int GridSize = 10;

    private static readonly Coordinate SourceCoordinate = new(0, 0);
    private static readonly Coordinate TargetCoordinate = new(GridSize - 1, GridSize - 1);
    private static readonly InclusiveValueRange<int> CostRange = new(1, 9);

    private static readonly int[,] CostMatrix = new int[GridSize, GridSize]
    {
        { 4, 7, 2, 8, 3, 6, 5, 9, 1, 2 },
        { 6, 5, 9, 4, 2, 7, 8, 3, 5, 3 },
        { 7, 3, 8, 6, 9, 5, 4, 2, 6, 4 },
        { 5, 8, 4, 7, 6, 9, 3, 4, 7, 5 },
        { 8, 9, 6, 5, 7, 2, 6, 5, 8, 6 },
        { 9, 4, 7, 3, 8, 6, 7, 9, 2, 5 },
        { 3, 6, 5, 9, 4, 8, 2, 7, 3, 6 },
        { 2, 7, 3, 5, 9, 4, 5, 6, 4, 7 },
        { 1, 8, 2, 4, 5, 3, 9, 8, 6, 8 },
        { 5, 9, 4, 6, 3, 5, 8, 4, 7, 9 },
    };

    private static readonly bool[,] ObstacleMatrix = new bool[GridSize, GridSize]
    {
        { false, false, false, true,  false, false, false, false, false, false },
        { false, false, false, false, true,  false, false, true,  false, false },
        { false, false, false, false, false, false, true,  false, false, false },
        { false, false, true,  false, false, false, false, false, false, false },
        { false, false, false, false, false, true,  false, false, false, false },
        { false, false, false, false, false, false, false, false, true,  false },
        { false, true,  false, false, false, false, false, false, false, false },
        { false, false, false, false, false, false, false, true,  false, false },
        { false, false, false, false, true,  false, false, false, false, false },
        { false, false, false, false, false, false, false, false, false, false },
    };

    private static readonly ReadOnlyCollection<Coordinate> PathCoordinates = Array.AsReadOnly(new[]
    {
        new Coordinate(0, 0),
        new Coordinate(1, 0),
        new Coordinate(2, 0),
        new Coordinate(3, 0),
        new Coordinate(4, 0),
        new Coordinate(5, 0),
        new Coordinate(6, 0),
        new Coordinate(7, 0),
        new Coordinate(8, 0),
        new Coordinate(9, 0),
        new Coordinate(9, 1),
        new Coordinate(9, 2),
        new Coordinate(9, 3),
        new Coordinate(9, 4),
        new Coordinate(9, 5),
        new Coordinate(9, 6),
        new Coordinate(9, 7),
        new Coordinate(9, 8),
        new Coordinate(9, 9),
    });

    private static readonly double ExpectedPathCost = CalculatePathCost(PathCoordinates);

    public static int ExpectedPathLength => PathCoordinates.Count - 1;

    public static TestGraph CreateGraph()
    {
        var graph = AssembleGraph(new MatrixLayer(CostMatrix, ObstacleMatrix));
        return CreateTestGraph(graph);
    }

    public static IReadOnlyList<Coordinate> GetExpectedPathCoordinates()
    {
        return PathCoordinates;
    }

    public static double GetExpectedPathCost() => ExpectedPathCost;

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
        var source = (IPathfindingVertex)graph.Get(SourceCoordinate);
        var target = (IPathfindingVertex)graph.Get(TargetCoordinate);
        var vertices = graph
            .Where(vertex => !vertex.IsObstacle)
            .Cast<IPathfindingVertex>()
            .ToList();

        return new TestGraph(source, target, vertices);
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
                vertex.Cost = new VertexCost(costs[x, y], CostRange);
            }
        }
    }

    private sealed class TestVertex : IVertex, IPathfindingVertex
    {
        private TestVertex[] neighbours = Array.Empty<TestVertex>();

        public bool IsObstacle { get; set; }

        public IVertexCost Cost { get; set; } = new VertexCost(1, CostRange);

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

internal sealed class TestGraph
{
    public TestGraph(
        IPathfindingVertex source,
        IPathfindingVertex target,
        IReadOnlyCollection<IPathfindingVertex> vertices)
    {
        Source = source;
        Target = target;
        Vertices = vertices;
        Range = new[] { Source, Target };
    }

    public IPathfindingVertex Source { get; }

    public IPathfindingVertex Target { get; }

    public IReadOnlyCollection<IPathfindingVertex> Vertices { get; }

    public IReadOnlyCollection<IPathfindingVertex> Range { get; }
}
