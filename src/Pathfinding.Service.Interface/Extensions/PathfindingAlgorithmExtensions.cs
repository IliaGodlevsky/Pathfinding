namespace Pathfinding.Service.Interface.Extensions;

public static class PathfindingAlgorithmExtensions
{
    public static async ValueTask<IGraphPath> FindPathAsync(this IPathfindingAlgorithm<IGraphPath> algorithm,
        CancellationToken token = default)
    {
        return await Task.Run(algorithm.FindPath, token).ConfigureAwait(false);
    }
}
