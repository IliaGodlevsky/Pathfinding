using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Data.InMemory.Repositories
{
    internal sealed class InMemoryStatisicsRepository : IStatisticsRepository
    {
        private int id = 0;
        private readonly HashSet<Statistics> set = new(EntityComparer<int>.Instance);

        public Task<Statistics> CreateAsync(Statistics entity, CancellationToken token = default)
        {
            entity.Id = ++id;
            set.Add(entity);
            return Task.FromResult(entity);
        }

        public Task<IEnumerable<Statistics>> CreateAsync(IEnumerable<Statistics> statistics, CancellationToken token = default)
        {
            foreach (var entity in statistics)
            {
                entity.Id = ++id;
                set.Add(entity);
            }
            return Task.FromResult(statistics);
        }

        public Task<IEnumerable<Statistics>> ReadByGraphIdAsync(int graphId, CancellationToken token = default)
        {
            var results = set.Where(s => s.GraphId == graphId).ToList();
            return Task.FromResult((IEnumerable<Statistics>)results);
        }

        public Task<int> ReadStatisticsCountAsync(int graphId, CancellationToken token = default)
        {
            var count = set.Count(s => s.GraphId == graphId);
            return Task.FromResult(count);
        }

        public Task<bool> DeleteByGraphId(int graphId, CancellationToken token = default)
        {
            var removed = set.RemoveWhere(s => s.GraphId == graphId) > 0;
            return Task.FromResult(removed);
        }

        public Task<bool> DeleteByIdsAsync(IEnumerable<int> ids, CancellationToken token = default)
        {
            var removed = set.RemoveWhere(s => ids.Contains(s.Id)) > 0;
            return Task.FromResult(removed);
        }

        public async Task<Statistics> ReadByIdAsync(int id, CancellationToken token = default)
        {
            var tracking = new Statistics() { Id = id };
            set.TryGetValue(tracking, out var statistics);
            return await Task.FromResult(statistics);
        }

        public async Task<bool> UpdateAsync(IEnumerable<Statistics> entities, CancellationToken token = default)
        {
            foreach (var entity in entities)
            {
                if (set.TryGetValue(entity, out var statistics))
                {
                    statistics.StepRule = entity.StepRule;
                    statistics.Steps = entity.Steps;
                    statistics.Heuristics = entity.Heuristics;
                    statistics.ResultStatus = entity.ResultStatus;
                    statistics.Cost = entity.Cost;
                    statistics.Algorithm = entity.Algorithm;
                    statistics.Weight = entity.Weight;
                    statistics.Visited = entity.Visited;
                    statistics.Elapsed = entity.Elapsed;
                }
            }
            return await Task.FromResult(true);
        }

        public async Task<IEnumerable<Statistics>> ReadByIdsAsync(IEnumerable<int> runIds, CancellationToken token = default)
        {
            var ids = runIds.ToHashSet();
            var statistics = set.Where(x => runIds.Contains(x.Id)).ToList();
            return await Task.FromResult(statistics);
        }
    }
}
