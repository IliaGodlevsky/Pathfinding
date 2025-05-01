using Pathfinding.Domain.Interface;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;
using System.Collections;

namespace Pathfinding.Infrastructure.Data.Pathfinding
{
    public abstract class Neighborhood : INeighborhood
    {
        protected readonly Coordinate selfCoordinate;
        private readonly int limitDepth;
        private readonly int[] resultCoordinatesValues;
        private readonly Lazy<IReadOnlyCollection<Coordinate>> neighbourhood;

        public int Count => neighbourhood.Value.Count;

        protected Neighborhood(Coordinate coordinate)
        {
            selfCoordinate = coordinate;
            limitDepth = selfCoordinate.Count;
            resultCoordinatesValues = new int[limitDepth];
            neighbourhood = new Lazy<IReadOnlyCollection<Coordinate>>(GetNeighborhood, true);
        }

        private HashSet<Coordinate> CollectNeighbors(int depth = 0)
        {
            var neighborhood = new HashSet<Coordinate>();
            for (int offset = -1; offset <= 1; offset++)
            {
                resultCoordinatesValues[depth] = selfCoordinate[depth] + offset;
                var neighbours = IsBottom(depth)
                    ? CollectNeighbors(depth + 1).AsEnumerable()
                    : Filter(new(resultCoordinatesValues));
                neighborhood.AddRange(neighbours);
            }
            return neighborhood;
        }

        protected abstract Coordinate[] Filter(Coordinate coordinate);

        private bool IsBottom(int depth)
        {
            return depth < limitDepth - 1;
        }

        private IReadOnlyCollection<Coordinate> GetNeighborhood()
        {
            var coordinates = CollectNeighbors();
            coordinates.Remove(selfCoordinate);
            return coordinates;
        }

        public IEnumerator<Coordinate> GetEnumerator()
        {
            return neighbourhood.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
