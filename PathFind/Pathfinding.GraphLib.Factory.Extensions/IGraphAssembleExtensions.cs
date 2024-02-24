﻿using Pathfinding.GraphLib.Core.Interface;
using Pathfinding.GraphLib.Factory.Interface;
using Shared.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pathfinding.GraphLib.Factory.Extensions
{
    public static class IGraphAssembleExtensions
    {
        public static IGraph<TVertex> AssembleGraph<TVertex>(this IGraphAssemble<TVertex> self,
            ILayer layer, IReadOnlyList<int> dimensionSizes)
            where TVertex : IVertex
        {
            var graph = self.AssembleGraph(dimensionSizes);
            layer.Overlay((IGraph<IVertex>)graph);
            return graph;
        }

        public static IGraph<TVertex> AssembleGraph<TVertex>(this IGraphAssemble<TVertex> self,
            ILayer layer, params int[] dimensionSizes)
            where TVertex : IVertex
        {
            return self.AssembleGraph(layer, (IReadOnlyList<int>)dimensionSizes);
        }

        public static async Task<IGraph<TVertex>> AssembleGraphAsync<TVertex>(this IGraphAssemble<TVertex> self,
            ILayer layer, params int[] dimensionSizes)
            where TVertex : IVertex
        {
            return await self.AssembleGraphAsync(layer, (IReadOnlyList<int>)dimensionSizes);
        }

        public static async Task<IGraph<TVertex>> AssembleGraphAsync<TVertex>(this IGraphAssemble<TVertex> self,
            ILayer layer, IReadOnlyList<int> dimensionSizes)
            where TVertex : IVertex
        {
            return await Task.Run(() => self.AssembleGraph(layer, dimensionSizes));
        }
    }
}