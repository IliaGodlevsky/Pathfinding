using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;
using Pathfinding.Shared.Extensions;

namespace Pathfinding.Infrastructure.Data.InMemory.Repositories
{
    internal sealed class InMemoryRangeRepository : IRangeRepository
    {
        private int id = 0;

        private readonly HashSet<PathfindingRange> set = new(EntityComparer<int>.Instance);

        public async Task<IEnumerable<PathfindingRange>> CreateAsync(IEnumerable<PathfindingRange> entities,
            CancellationToken token = default)
        {
            var result = entities
                .ForEach(x => x.Id = ++id)
                .ForWhole(set.AddRange)
                .ToList();
            return await Task.FromResult(result);
        }

        public async Task<bool> DeleteByGraphIdAsync(int graphId,
            CancellationToken token = default)
        {
            var result = set.RemoveWhere(x => x.GraphId == graphId);
            return await Task.FromResult(result > 0);
        }

        public async Task<bool> DeleteByVerticesIdsAsync(IEnumerable<long> verticesIds,
            CancellationToken token = default)
        {
            var result = set.RemoveWhere(x => verticesIds.Contains(x.VertexId));
            return await Task.FromResult(result > 0);
        }

        public async Task<IEnumerable<PathfindingRange>> ReadByGraphIdAsync(int graphId,
            CancellationToken token = default)
        {
            var result = set.Where(x => x.GraphId == graphId)
                .OrderBy(x => x.Order)
                .ToList();
            return await Task.FromResult(result);
        }

        public async Task<IEnumerable<PathfindingRange>> UpsertAsync(IEnumerable<PathfindingRange> entities, CancellationToken token = default)
        {
            foreach (var entity in entities)
            {
                if (set.TryGetValue(entity, out var value))
                {
                    set.Remove(value);
                    set.Add(entity);
                }
                else
                {
                    entity.Id = ++id;
                    set.Add(entity);
                }
            }
            return await Task.FromResult(entities);
        }
    }
}
