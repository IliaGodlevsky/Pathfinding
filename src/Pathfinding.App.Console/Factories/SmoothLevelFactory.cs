using Autofac.Features.Metadata;
using Pathfinding.App.Console.Injection;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Layers;

namespace Pathfinding.App.Console.Factories;

internal sealed class SmoothLevelFactory(Meta<SmoothLayer>[] layers) : ISmoothLevelFactory
{
    private readonly Dictionary<SmoothLevels, SmoothLayer> layers = layers.ToDictionary(
            x => (SmoothLevels)x.Metadata[MetadataKeys.SmoothLevels],
            x => x.Value);

    public IReadOnlyCollection<SmoothLevels> Allowed => layers.Keys;

    public SmoothLayer CreateLayer(SmoothLevels level)
    {
        return layers.TryGetValue(level, out var value)
            ? value
            : throw new KeyNotFoundException($"{level} was not found");
    }
}