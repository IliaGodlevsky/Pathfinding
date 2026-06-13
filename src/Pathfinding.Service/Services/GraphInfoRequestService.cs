using Pathfinding.Data.InMemory;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Extensions;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Shared.Extensions;

namespace Pathfinding.Service.Services;

public sealed class GraphInfoRequestService(IUnitOfWorkFactory factory) : IGraphInfoRequestService
{
    public GraphInfoRequestService() : this(new InMemoryUnitOfWorkFactory())
    {
    }

    public Task<IReadOnlyCollection<GraphInformationModel>> ReadAllGraphInfoAsync(
        CancellationToken token = default)
    {
        return ExecuteAsync(async (unitOfWork, t) =>
        {
            var graphs = await unitOfWork.GraphRepository
                .GetAll()
                .ToListAsync(t)
                .ConfigureAwait(false);
            var graphIds = graphs.Select(x => x.Id).ToHashSet();
            var obstaclesCount = await unitOfWork.GraphRepository
                .ReadObstaclesCountAsync(graphIds, t)
                .ConfigureAwait(false);
            var infos = graphs.ToInformationModels();
            infos.ForEach(x => x.ObstaclesCount = obstaclesCount.GetValueOrDefault(x.Id));
            return infos;
        }, token);
    }

    public Task<GraphInformationModel> ReadGraphInfoAsync(
        int graphId,
        CancellationToken token = default)
    {
        return ExecuteAsync(async (unitOfWork, t) =>
        {
            var result = await unitOfWork.GraphRepository
                .ReadAsync(graphId, t)
                .ConfigureAwait(false);
            return result.ToGraphInformationModel();
        }, token);
    }

    public Task<bool> UpdateGraphInfoAsync(
        GraphInformationModel graph,
        CancellationToken token = default)
    {
        return ExecuteAsync((unit, t) =>
        {
            var graphInfo = graph.ToGraphEntity();
            return unit.GraphRepository.UpdateAsync(graphInfo, t);
        }, token);
    }

    public Task<bool> DeleteGraphsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken token = default)
    {
        return ExecuteAsync(
            (unitOfWork, t) => unitOfWork.GraphRepository.DeleteAsync(ids, t),
            token);
    }

    private async Task<TResult> ExecuteAsync<TResult>(
        Func<IUnitOfWork, CancellationToken, Task<TResult>> action,
        CancellationToken token)
    {
        await using var unit = await factory.CreateAsync(token).ConfigureAwait(false);
        return await action(unit, token).ConfigureAwait(false);
    }
}
