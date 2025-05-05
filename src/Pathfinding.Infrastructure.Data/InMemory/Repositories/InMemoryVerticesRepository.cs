using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;
using Pathfinding.Shared.Extensions;

namespace Pathfinding.Infrastructure.Data.InMemory.Repositories
{
    internal sealed class InMemoryVerticesRepository : IVerticesRepository
    {
        private long id;

        private readonly HashSet<Vertex> set = new(EntityComparer<long>.Instance);

        public async Task<IReadOnlyCollection<Vertex>> CreateAsync(
            IReadOnlyCollection<Vertex> vertices,
            CancellationToken token = default)
        {
            var result = vertices
                .ForEach(x => x.Id = ++id)
                .ForWhole(set.AddRange)
                .ToList();
            return await Task.FromResult(result);
        }

        public async Task<bool> DeleteVerticesByGraphIdAsync(int graphId)
        {
            var result = set.RemoveWhere(x => x.GraphId == graphId);
            return await Task.FromResult(result > 0);
        }

        public async Task<Vertex> ReadAsync(long vertexId,
            CancellationToken token = default)
        {
            var vertex = new Vertex { Id = vertexId };
            set.TryGetValue(vertex, out var result);
            return await Task.FromResult(result);
        }

        public IAsyncEnumerable<Vertex> ReadVerticesByGraphIdAsync(int graphId)
        {
            return set
                .Where(x => x.GraphId == graphId)
                .ToAsyncEnumerable();
        }

        public IAsyncEnumerable<Vertex> ReadAsync(
            IReadOnlyCollection<long> verticesIds)
        {
            var vertices = verticesIds
                .Select(x => new Vertex { Id = x })
                .ToArray();
            var range = vertices.Select(x =>
                {
                    set.TryGetValue(x, out var got);
                    return got;
                })
                .Where(x => x is not null)
                .ToList();
            return range.ToAsyncEnumerable();
        }

        public Task<bool> UpdateVerticesAsync(
            IReadOnlyCollection<Vertex> vertices,
            CancellationToken token = default)
        {
            foreach (var vertex in vertices)
            {
                if (set.TryGetValue(vertex, out var result))
                {
                    set.Remove(result);
                    set.Add(vertex);
                }
            }
            return Task.FromResult(true);
        }
    }
}
