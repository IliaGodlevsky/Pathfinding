using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface;

namespace Pathfinding.Data.Extensions;

public static class GraphAssembleExtensions
{
    public static async Task<IGraph<TVertex>> AssembleGraphAsync<TVertex>(this IGraphAssemble<TVertex> self,
        ILayer layer, IReadOnlyList<int> dimensionSizes, CancellationToken token = default)
        where TVertex : IVertex
    {
        var graph = self.AssembleGraph(dimensionSizes);
        await Task
            .Run(() => layer.Overlay((IGraph<IVertex>)graph), token)
            .ConfigureAwait(false);
        return graph;
    }
}