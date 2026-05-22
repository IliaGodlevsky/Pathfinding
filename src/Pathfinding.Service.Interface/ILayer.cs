using Pathfinding.Domain.Interface;

namespace Pathfinding.Service.Interface;

public interface ILayer
{
    void Overlay(IGraph<IVertex> graph);
}
