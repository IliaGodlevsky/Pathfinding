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

    public IReadOnlyCollection<SmoothLevels> AvailableLevels => layers.Keys;

    public SmoothLayer CreateLayer(SmoothLevels level)
    {
        return layers.GetValueOrDefault(level)
            ?? throw new KeyNotFoundException($"{level} was not found");
    }
}