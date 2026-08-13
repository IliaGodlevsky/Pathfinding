using Autofac.Features.Metadata;
using Pathfinding.Domain.Enums;

namespace Pathfinding.Presentation.Console.Factories;

internal sealed class ObstaclesLayersFactory(Meta<IObstaclesLayerFactory>[] factories) : IObstaclesLayersFactory
{
    private readonly Dictionary<GraphGenerators, IObstaclesLayerFactory> factories
        = factories.ToDictionary(x => (GraphGenerators)x.Metadata["Generator"], x => x.Value);

    public IReadOnlyList<GraphGenerators> AvailableGenerators { get; }
        = [.. factories.Select(x => (GraphGenerators)x.Metadata["Generator"])];

    public IObstaclesLayerFactory GetObstacleLayer(GraphGenerators generator)
    {
        return factories.GetValueOrDefault(generator) 
            ?? throw new KeyNotFoundException("Not such a generator");
    }
}
