using Autofac.Features.AttributeFilters;
using Autofac.Features.Metadata;
using Pathfinding.Domain.Enums;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Service.Layers;

namespace Pathfinding.Presentation.Console.Factories;

public sealed class NeighborhoodLayerFactory(
    [KeyFilter(KeyFilters.Neighborhoods)] Meta<NeighborhoodLayer>[] layers) : INeighborhoodLayerFactory
{
    private readonly Dictionary<Neighborhoods, NeighborhoodLayer> layers
        = layers.ToDictionary(x => (Neighborhoods)x.Metadata[MetadataKeys.Neighborhoods], x => x.Value);

    public IReadOnlyCollection<Neighborhoods> AvailableNeighborhoods => layers.Keys;

    public NeighborhoodLayer Create(Neighborhoods neighborhoods)
    {
        return layers.GetValueOrDefault(neighborhoods)
            ?? throw new KeyNotFoundException($"{neighborhoods} was not found");
    }
}