using Pathfinding.Domain.Interface;

namespace Pathfinding.Service.Interface;

public interface IGraphAssemble<out TVertex>
    where TVertex : IVertex
{
    IGraph<TVertex> AssembleGraph(IReadOnlyList<int> graphDimensionsSizes);
}