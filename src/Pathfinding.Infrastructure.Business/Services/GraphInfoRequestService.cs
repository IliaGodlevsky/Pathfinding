using Pathfinding.Domain.Interface.Extensions;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Infrastructure.Data.InMemory;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Shared.Extensions;

namespace Pathfinding.Infrastructure.Business.Services;

public sealed class GraphInfoRequestService(IUnitOfWorkFactory factory) : IGraphInfoRequestService
{
    public GraphInfoRequestService() : this(new InMemoryUnitOfWorkFactory())
    {
    }

    public async Task<IReadOnlyCollection<GraphInformationModel>> ReadAllGraphInfoAsync(
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var result = await unitOfWork.GraphRepository
                .GetAll()
                .ToListAsync(t)
                .ConfigureAwait(false);
            var ids = result.Select(x => x.Id).ToHashSet();
            var obstaclesCount = await unitOfWork.GraphRepository
                .ReadObstaclesCountAsync(ids, t)
                .ConfigureAwait(false);
            var infos = result.ToInformationModels();
            infos.ForEach(x => x.ObstaclesCount = obstaclesCount[x.Id]);
            return infos;
        }, token).ConfigureAwait(false);
    }

    public async Task<GraphInformationModel> ReadGraphInfoAsync(
        int graphId,
        CancellationToken token = default)
    {
        await using var unitOfWork = await factory
            .CreateAsync(token)
            .ConfigureAwait(false);
        var result = await unitOfWork.GraphRepository
            .ReadAsync(graphId, token)
            .ConfigureAwait(false);
        return result.ToGraphInformationModel();
    }

    public async Task<bool> UpdateGraphInfoAsync(
        GraphInformationModel graph,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var graphInfo = graph.ToGraphEntity();
            return await unit.GraphRepository
                .UpdateAsync(graphInfo, t)
                .ConfigureAwait(false);
        }, token).ConfigureAwait(false);
    }

    public async Task<bool> DeleteGraphsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            return await unitOfWork.GraphRepository
                .DeleteAsync(ids, t)
                .ConfigureAwait(false);
        }, token).ConfigureAwait(false);
    }
}
