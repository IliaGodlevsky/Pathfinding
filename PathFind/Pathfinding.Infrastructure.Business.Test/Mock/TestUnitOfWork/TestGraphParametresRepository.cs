﻿#nullable disable
using Pathfinding.Domain.Core;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Business.Test.Mock.TestUnitOfWork
{
    internal sealed class TestGraphParametresRepository : IGraphParametresRepository
    {
        private int id = 0;

        private readonly HashSet<Graph> set = new(EntityComparer<int>.Interface);

        public Task<Graph> CreateAsync(Graph graph, CancellationToken token = default)
        {
            graph.Id = ++id;
            set.Add(graph);
            return Task.FromResult(graph);
        }

        public Task<bool> DeleteAsync(int graphId, CancellationToken token = default)
        {
            var deleted = set.RemoveWhere(x => x.Id == graphId);
            return Task.FromResult(deleted == 1);
        }

        public async Task<bool> DeleteAsync(IEnumerable<int> graphIds, CancellationToken token = default)
        {
            var ids = graphIds.ToHashSet();
            var deleted = set.RemoveWhere(x => ids.Contains(x.Id));
            return await Task.FromResult(deleted == ids.Count);
        }

        public IAsyncEnumerable<Graph> GetAll(CancellationToken token = default)
        {
            return set.ToAsyncEnumerable();
        }

        public async Task<Graph> ReadAsync(int graphId, CancellationToken token = default)
        {
            var equal = new Graph() { Id = graphId };
            set.TryGetValue(equal, out var result);
            return await Task.FromResult(result);
        }

        public async Task<int> ReadCountAsync(CancellationToken token = default)
        {
            return await Task.FromResult(set.Count);
        }

        public async Task<bool> UpdateAsync(Graph graph, CancellationToken token = default)
        {
            var equal = new Graph { Id = graph.Id };
            if (set.TryGetValue(equal, out var result))
            {
                result.ObstaclesCount = graph.ObstaclesCount;
                result.Dimensions = graph.Dimensions;
                result.Name = graph.Name;
                return await Task.FromResult(true);
            }
            return false;
        }
    }
}
