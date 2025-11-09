using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Business.Layers;
using Pathfinding.Infrastructure.Data.Extensions;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

internal static class TestGraphFactory
{
    private const int GridSize = 10;

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

    public static TestGraph CreateGraph()
    {
        var matrices = new MatrixLayer(CostMatrix, ObstacleMatrix);
        var neighborhoodLayer = new MooreNeighborhoodLayer();
        var layers = new Layers.Layers(matrices, neighborhoodLayer);
        var graph = new GraphAssemble<TestVertex>().AssembleGraph([GridSize, GridSize]);
        layers.Overlay(graph);
        var source = graph.Get(0, 0);
        var target = graph.Get(GridSize - 1, GridSize - 1);

        return new TestGraph(source, target, graph);
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
        private TestVertex[] neighbours = [];

        public bool IsObstacle { get; set; }

        public IVertexCost Cost { get; set; } = new VertexCost(1, CostRange);

        public Coordinate Position { get; set; } = new Coordinate(0, 0);

        public IReadOnlyCollection<IVertex> Neighbors
        {
            get => neighbours;
            set => neighbours = value?.Cast<TestVertex>().ToArray() ?? [];
        }

        IReadOnlyCollection<IPathfindingVertex> IPathfindingVertex.Neighbors => neighbours;

        public bool Equals(IVertex other)
        {
            return other is not null && Position.Equals(other.Position);
        }

        public override bool Equals(object obj)
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
        Range = [Source, Target];
    }

    public IPathfindingVertex Source { get; }

    public IPathfindingVertex Target { get; }

    public IReadOnlyCollection<IPathfindingVertex> Vertices { get; }

    public IReadOnlyCollection<IPathfindingVertex> Range { get; }
}
