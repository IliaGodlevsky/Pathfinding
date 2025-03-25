namespace Pathfinding.Domain.Interface;

public interface ILayer
{
    void Overlay(IGraph<IVertex> graph);
}
