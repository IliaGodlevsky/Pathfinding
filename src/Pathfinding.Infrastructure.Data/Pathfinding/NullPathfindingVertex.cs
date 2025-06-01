using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;
using System.Diagnostics;

namespace Pathfinding.Infrastructure.Data.Pathfinding;

[DebuggerDisplay("Null")]
public sealed class NullPathfindingVertex : Singleton<NullPathfindingVertex, IPathfindingVertex>,
    IPathfindingVertex
{
    public bool IsObstacle => true;

    public IVertexCost Cost => NullCost.Interface;

    public IReadOnlyCollection<IPathfindingVertex> Neighbors => [];

    public Coordinate Position => Coordinate.Empty;

    private NullPathfindingVertex()
    {

    }
}