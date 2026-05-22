using Pathfinding.Data;
using Pathfinding.Data.Extensions;
using Pathfinding.Domain;
using Pathfinding.Domain.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Service.Tests;

internal class FakeVertex : IVertex, IEntity<long>
{
    public long Id { get; set; }

    public bool IsObstacle { get; set; }

    public IVertexCost Cost { get; set; } = new VertexCost(1);

    public Coordinate Position { get; set; }

    public IReadOnlyCollection<IVertex> Neighbors { get; set; } = [];

    public bool Equals(IVertex other) => this.IsEqual(other);
}