namespace Pathfinding.Domain.Core;

public interface IEntity<TId>
{
    TId Id { get; set; }
}
