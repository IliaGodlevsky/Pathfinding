using Pathfinding.Data.Neighborhoods;
using Pathfinding.Domain.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Service.Layers;

public sealed class KnightsNeighborhoodLayer : NeighborhoodLayer
{
    protected override INeighborhood CreateNeighborhood(Coordinate coordinate)
    {
        return new KnightNeighborhood(coordinate);
    }
}