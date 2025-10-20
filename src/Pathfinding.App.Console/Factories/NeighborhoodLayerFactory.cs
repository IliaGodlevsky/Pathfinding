using Autofac.Features.AttributeFilters;
using Autofac.Features.Metadata;
using Pathfinding.App.Console.Injection;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Layers;

namespace Pathfinding.App.Console.Factories;

public sealed class NeighborhoodLayerFactory(
    [KeyFilter(KeyFilters.Neighborhoods)] Meta<NeighborhoodLayer>[] layers) : INeighborhoodLayerFactory
{
    private readonly Dictionary<Neighborhoods, NeighborhoodLayer> layers
        = layers.ToDictionary(x => (Neighborhoods)x.Metadata[MetadataKeys.Neighborhoods], x => x.Value);

    public IReadOnlyCollection<Neighborhoods> Allowed => layers.Keys;

    public NeighborhoodLayer CreateNeighborhoodLayer(Neighborhoods neighborhoods)
    {
        return layers.TryGetValue(neighborhoods, out var value)
            ? value
            : throw new KeyNotFoundException($"{neighborhoods} was not found");
    }
}