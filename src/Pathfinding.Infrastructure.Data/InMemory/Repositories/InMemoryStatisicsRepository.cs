using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Data.InMemory.Repositories
{
    internal sealed class InMemoryStatisicsRepository : IStatisticsRepository
    {
        private int id;
        private readonly HashSet<Statistics> set = new(EntityComparer<int>.Instance);


        public Task<IReadOnlyCollection<Statistics>> CreateAsync(
            IReadOnlyCollection<Statistics> statistics, 
            CancellationToken token = default)
        {
            foreach (var entity in statistics)
            {
                entity.Id = ++id;
                set.Add(entity);
            }
            return Task.FromResult(statistics);
        }

        public IAsyncEnumerable<Statistics> ReadByGraphIdAsync(int graphId)
        {
            return set.Where(s => s.GraphId == graphId).ToAsyncEnumerable();
        }

        public Task<bool> DeleteByGraphId(int graphId)
        {
            var removed = set.RemoveWhere(s => s.GraphId == graphId) > 0;
            return Task.FromResult(removed);
        }

        public Task<bool> DeleteByIdsAsync(
            IReadOnlyCollection<int> ids, 
            CancellationToken token = default)
        {
            var removed = set.RemoveWhere(s => ids.Contains(s.Id)) > 0;
            return Task.FromResult(removed);
        }

        public async Task<Statistics> ReadByIdAsync(
            int statId,
            CancellationToken token = default)
        {
            var tracking = new Statistics { Id = statId };
            set.TryGetValue(tracking, out var statistics);
            return await Task.FromResult(statistics);
        }

        public async Task<bool> UpdateAsync(
            IReadOnlyCollection<Statistics> entities,
            CancellationToken token = default)
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

        public IAsyncEnumerable<Statistics> ReadByIdsAsync(
            IReadOnlyCollection<int> runIds)
        {
            return set
                .Where(x => runIds.Contains(x.Id))
                .ToAsyncEnumerable();
        }
    }
}
