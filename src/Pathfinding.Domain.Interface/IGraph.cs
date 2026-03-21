using Pathfinding.Shared.Primitives;

namespace Pathfinding.Domain.Interface;

public interface IGraph<out TVertex> : IReadOnlyCollection<TVertex>, ILayer
    where TVertex : IVertex
{
    InclusiveValueRange<int> CostRange { get; set; }

    IReadOnlyList<int> DimensionsSizes { get; }

    TVertex Get(Coordinate coordinate);
}