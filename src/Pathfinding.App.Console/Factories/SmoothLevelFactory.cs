using Autofac.Features.Metadata;
using Pathfinding.App.Console.Injection;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Layers;

namespace Pathfinding.App.Console.Factories;

internal sealed class SmoothLevelFactory : ISmoothLevelFactory
{
    private readonly Dictionary<SmoothLevels, SmoothLayer> layers;

    public IReadOnlyCollection<SmoothLevels> Allowed => layers.Keys;

    public SmoothLevelFactory(Meta<SmoothLayer>[] layers)
    {
        this.layers = layers.ToDictionary(
            x => (SmoothLevels)x.Metadata[MetadataKeys.SmoothLevels],
            x => x.Value);
    }

    public SmoothLayer CreateLayer(SmoothLevels level)
    {
        if (layers.TryGetValue(level, out var value))
        {
            return value;
        }

        throw new KeyNotFoundException($"{level} was not found");
    }
}