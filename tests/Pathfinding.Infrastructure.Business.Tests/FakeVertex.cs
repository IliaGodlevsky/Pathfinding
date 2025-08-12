using Pathfinding.Domain.Core;
using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Data.Extensions;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Tests;

internal class FakeVertex : IVertex, IEntity<long>
{
    public long Id { get; set; }

    public bool IsObstacle { get; set; }

    public IVertexCost Cost { get; set; } = new VertexCost(1, (1, 9));

    public Coordinate Position { get; set; }

    public IReadOnlyCollection<IVertex> Neighbors { get; set; } = [];

    public bool Equals(IVertex other) => this.IsEqual(other);
}