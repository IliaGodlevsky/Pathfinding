using Autofac.Features.AttributeFilters;
using Autofac.Features.Metadata;
using Pathfinding.App.Console.Injection;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;

namespace Pathfinding.App.Console.Factories;

public sealed class NeighborhoodLayerFactory : INeighborhoodLayerFactory
{
    private readonly Dictionary<Neighborhoods, ILayer> layers;

    public IReadOnlyCollection<Neighborhoods> Allowed => layers.Keys;

    public NeighborhoodLayerFactory(
        [KeyFilter(KeyFilters.Neighborhoods)] Meta<ILayer>[] layers)
    {
        this.layers = layers.ToDictionary(x => (Neighborhoods)x.Metadata[MetadataKeys.Neighborhoods], x => x.Value);
    }

    public ILayer CreateNeighborhoodLayer(Neighborhoods neighborhoods)
    {
        if (layers.TryGetValue(neighborhoods, out var value))
        {
            return value;
        }

        throw new KeyNotFoundException($"{neighborhoods} was not found");
    }
}