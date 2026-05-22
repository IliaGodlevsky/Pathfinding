using Pathfinding.Data.Neighborhoods;
using Pathfinding.Domain.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Service.Layers;

public class DiagonalNeighborhoodLayer : NeighborhoodLayer
{
    protected override INeighborhood CreateNeighborhood(Coordinate coordinate)
    {
        return new DiagonalNeighborhood(coordinate);
    }
}