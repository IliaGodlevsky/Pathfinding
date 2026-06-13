using Pathfinding.Data.InMemory;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Extensions;
using Pathfinding.Service.Extensions;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Shared.Extensions;

namespace Pathfinding.Service.Services;

public sealed class GraphInfoRequestService(IUnitOfWorkFactory factory)
    : IGraphInfoRequestService
{
    public GraphInfoRequestService()
        : this(new InMemoryUnitOfWorkFactory())
    {
    }

    public ValueTask<IReadOnlyCollection<GraphInformationModel>> ReadAllGraphInfoAsync(
        CancellationToken token = default)
    {
        return factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var graphs = await unitOfWork.GraphRepository
                .GetAll()
                .ToListAsync(t)
                .ConfigureAwait(false);

            var ids = graphs.Select(x => x.Id).ToHashSet();

            var obstaclesCount = await unitOfWork.GraphRepository
                .ReadObstaclesCountAsync(ids, t)
                .ConfigureAwait(false);

            var infos = graphs.ToInformationModels();

            infos.ForEach(x => x.ObstaclesCount = obstaclesCount[x.Id]);

            return infos;
        }, token);
    }

    public async Task<GraphInformationModel> ReadGraphInfoAsync(
        int graphId,
        CancellationToken token = default)
    {
        await using var unitOfWork = await factory
            .CreateAsync(token)
            .ConfigureAwait(false);

        var graph = await unitOfWork.GraphRepository
            .ReadAsync(graphId, token)
            .ConfigureAwait(false);

        return graph.ToGraphInformationModel();
    }

    public ValueTask<bool> UpdateGraphInfoAsync(
        GraphInformationModel graph,
        CancellationToken token = default)
    {
        return factory.TransactionAsync((unitOfWork, t) =>
        {
            var graphEntity = graph.ToGraphEntity();
            return unitOfWork.GraphRepository.UpdateAsync(graphEntity, t);
        }, token);
    }

    public ValueTask<bool> DeleteGraphsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken token = default)
    {
        return factory.TransactionAsync(
            (unitOfWork, t) => unitOfWork.GraphRepository.DeleteAsync(ids, t),
            token);
    }
}
