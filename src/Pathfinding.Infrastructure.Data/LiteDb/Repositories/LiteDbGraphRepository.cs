using LiteDB;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Data.LiteDb.Repositories
{
    internal sealed class LiteDbGraphRepository : IGraphParametresRepository
    {
        private readonly ILiteCollection<Graph> collection;
        private readonly LiteDbRangeRepository rangeRepository;
        private readonly LiteDbVerticesRepository verticesRepository;
        private readonly LiteDbStatisticsRepository statisticsRepository;
        private readonly ILiteCollection<Vertex> vertexCollection;

        public LiteDbGraphRepository(ILiteDatabase db)
        {
            collection = db.GetCollection<Graph>(DbTables.Graphs);
            rangeRepository = new LiteDbRangeRepository(db);
            verticesRepository = new LiteDbVerticesRepository(db);
            statisticsRepository = new LiteDbStatisticsRepository(db);
            vertexCollection = db.GetCollection<Vertex>(DbTables.Vertices);
            collection.EnsureIndex(x => x.Id);
        }

        public async Task<Graph> CreateAsync(Graph graph, CancellationToken token = default)
        {
            collection.Insert(graph);
            return await Task.FromResult(graph);
        }

        public async Task<bool> DeleteAsync(int graphId, CancellationToken token = default)
        {
            // Order sensitive. Do not change the order of deleting
            // Reason: some repositories need the presence of values in the database
            await rangeRepository.DeleteByGraphIdAsync(graphId, token);
            await verticesRepository.DeleteVerticesByGraphIdAsync(graphId, token);
            await statisticsRepository.DeleteByGraphId(graphId, token);
            return collection.Delete(graphId);
        }

        public async Task<bool> DeleteAsync(IEnumerable<int> graphIds,
            CancellationToken token = default)
        {
            foreach (var id in graphIds)
            {
                await DeleteAsync(id, token);
            }
            return true;
        }

        public async Task<IEnumerable<Graph>> GetAll(CancellationToken token = default)
        {
            return await Task.FromResult(collection.FindAll());
        }

        public async Task<Graph> ReadAsync(int graphId, CancellationToken token = default)
        {
            return await Task.FromResult(collection.FindById(graphId));
        }

        public async Task<IReadOnlyDictionary<int, int>> ReadObstaclesCountAsync(IEnumerable<int> graphIds, CancellationToken token = default)
        {
            return await Task.FromResult(vertexCollection
                .Find(x => graphIds.Contains(x.GraphId) && x.IsObstacle)
                .GroupBy(x => x.GraphId)
                .ToDictionary(x => x.Key, x => x.Count()));
        }

        public async Task<bool> UpdateAsync(Graph graph, CancellationToken token = default)
        {
            return await Task.FromResult(collection.Update(graph));
        }
    }
}
