using Pathfinding.Service.Interface;
using Pathfinding.Service.Layers;

namespace Pathfinding.Presentation.Console.Factories;

internal sealed class PerfectMazeObstacleLayerFactory : IObstaclesLayerFactory
{
    public ILayer Create(int obstaclePercent, Random random)
    {
        return new MazeObstacleLayer(obstaclePercent, random);
    }
}
