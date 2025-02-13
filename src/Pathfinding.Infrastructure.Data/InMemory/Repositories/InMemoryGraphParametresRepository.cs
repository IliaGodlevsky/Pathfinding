using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Data.InMemory.Repositories
{
    internal sealed class InMemoryGraphParametresRepository(
        InMemoryRangeRepository rangeRepository,
        InMemoryVerticesRepository verticesRepository,
        InMemoryStatisicsRepository statisticsRepository) : IGraphParametresRepository
    {
        private int id = 0;

        private readonly HashSet<Graph> set = new(EntityComparer<int>.Interface);

        public Task<Graph> CreateAsync(Graph graph,
            CancellationToken token = default)
        {
            graph.Id = ++id;
            set.Add(graph);
            return Task.FromResult(graph);
        }

        public async Task<bool> DeleteAsync(int graphId,
            CancellationToken token = default)
        {
            // Order sensitive. Do not change the order of deleting
            // Reason: some repositories need the presence of values in the database
            await rangeRepository.DeleteByGraphIdAsync(graphId, token);
            await verticesRepository.DeleteVerticesByGraphIdAsync(graphId, token);
            await statisticsRepository.DeleteByGraphId(graphId, token);
            var deleted = set.RemoveWhere(x => x.Id == graphId);
            return await Task.FromResult(deleted == 1);
        }

        public async Task<bool> DeleteAsync(IEnumerable<int> graphIds,
            CancellationToken token = default)
        {
            foreach (var graphId in graphIds)
            {
                await DeleteAsync(graphId, token);
            }
            return true;
        }

        public async Task<IEnumerable<Graph>> GetAll(CancellationToken token = default)
        {
            return await Task.FromResult(set);
        }

        public async Task<Graph> ReadAsync(int graphId,
            CancellationToken token = default)
        {
            var equal = new Graph() { Id = graphId };
            set.TryGetValue(equal, out var result);
            return await Task.FromResult(result);
        }

        public async Task<bool> UpdateAsync(Graph graph,
            CancellationToken token = default)
        {
            var equal = new Graph { Id = graph.Id };
            if (set.TryGetValue(equal, out var result))
            {
                result.Dimensions = graph.Dimensions;
                result.Name = graph.Name;
                result.Neighborhood = graph.Neighborhood;
                result.SmoothLevel = graph.SmoothLevel;
                return await Task.FromResult(true);
            }
            return false;
        }

        public async Task<IReadOnlyDictionary<int, int>> ReadObstaclesCountAsync(IEnumerable<int> graphIds,
            CancellationToken token = default)
        {
            var result = new Dictionary<int, int>();
            foreach (var graph in graphIds)
            {
                var vertices = await verticesRepository.ReadVerticesByGraphIdAsync(graph, token);
                int obstacles = vertices.Where(x => x.IsObstacle).Count();
                result.Add(graph, obstacles);
            }
            return result;
        }
    }
}
