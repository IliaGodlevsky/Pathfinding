using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Data.Pathfinding
{
    public sealed class GraphAssemble<TVertex> : IGraphAssemble<TVertex>
        where TVertex : IVertex, new()
    {
        public IGraph<TVertex> AssembleGraph(IReadOnlyList<int> graphDimensionsSizes)
        {
            int graphSize = graphDimensionsSizes.AggregateOrDefault((x, y) => x * y);
            var vertices = Enumerable.Range(0, graphSize)
                .Select(i => new TVertex() { Position = ToCoordinates(graphDimensionsSizes, i) })
                .ToArray();
            return new Graph<TVertex>(vertices, graphDimensionsSizes);
        }

        private static Coordinate ToCoordinates(IReadOnlyList<int> dimensionSizes, int index)
        {
            var range = new InclusiveValueRange<int>(dimensionSizes.Count - 1);
            int Coordinate(int i)
            {
                var coordinate = index % dimensionSizes[i];
                index /= dimensionSizes[i];
                return coordinate;
            }
            var coordinates = range.Iterate().Select(Coordinate).ToArray();
            return new(coordinates);
        }
    }
}