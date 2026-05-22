using Autofac.Features.Metadata;
using Pathfinding.Domain.Enums;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Service.Layers;

namespace Pathfinding.Presentation.Console.Factories;

internal sealed class SmoothLevelFactory(Meta<SmoothLayer>[] layers) : ISmoothLevelFactory
{
    private readonly Dictionary<SmoothLevels, SmoothLayer> layers = layers.ToDictionary(
            x => (SmoothLevels)x.Metadata[MetadataKeys.SmoothLevels],
            x => x.Value);

    public IReadOnlyCollection<SmoothLevels> AvailableLevels => layers.Keys;

    public SmoothLayer Create(SmoothLevels level)
    {
        return layers.GetValueOrDefault(level)
            ?? throw new KeyNotFoundException($"{level} was not found");
    }
}