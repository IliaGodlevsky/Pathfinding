using Pathfinding.Data.InMemory;
using Pathfinding.Domain.Interface;
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
        await using var unitOfWork = await factory.CreateAsync(token).ConfigureAwait(false);
        var graphs = await unitOfWork.GraphRepository
            .GetAll()
            .ToListAsync(token)
            .ConfigureAwait(false);
        var graphIds = graphs.Select(x => x.Id).ToHashSet();
        var obstaclesCount = await unitOfWork.GraphRepository
            .ReadObstaclesCountAsync(graphIds, token)
            .ConfigureAwait(false);
        var infos = graphs.ToInformationModels();
        infos.ForEach(x => x.ObstaclesCount = obstaclesCount.GetValueOrDefault(x.Id));
        return infos;
    }

    public async Task<GraphInformationModel> ReadGraphInfoAsync(
        int graphId,
        CancellationToken token = default)
    {
        await using var unitOfWork = await factory.CreateAsync(token).ConfigureAwait(false);
        var result = await unitOfWork.GraphRepository.ReadAsync(graphId, token).ConfigureAwait(false);
        return result.ToGraphInformationModel();
    }

    public ValueTask<bool> UpdateGraphInfoAsync(
        GraphInformationModel graph,
        CancellationToken token = default)
    {
        await using var unit = await factory.CreateAsync(token).ConfigureAwait(false);
        var graphInfo = graph.ToGraphEntity();
        return await unit.GraphRepository.UpdateAsync(graphInfo, token).ConfigureAwait(false);
    }

    public ValueTask<bool> DeleteGraphsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken token = default)
    {
        await using var unitOfWork = await factory.CreateAsync(token).ConfigureAwait(false);
        return await unitOfWork.GraphRepository.DeleteAsync(ids, token).ConfigureAwait(false);
    }
}
