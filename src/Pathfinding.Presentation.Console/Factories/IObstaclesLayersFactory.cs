using Pathfinding.Domain.Enums;
using Pathfinding.Service.Interface;

namespace Pathfinding.Presentation.Console.Factories;

internal interface IObstaclesLayerFactory
{
    ILayer Create(int obstaclePercent, Random random);
}

internal interface IObstaclesLayersFactory
{
    IReadOnlyList<GraphGenerators> AvailableGenerators { get; }

    IObstaclesLayerFactory GetObstacleLayer(GraphGenerators generator);
}
