using Autofac.Features.AttributeFilters;
using Autofac.Features.Metadata;
using Pathfinding.App.Console.Injection;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;

namespace Pathfinding.App.Console.Factories;

public sealed class NeighborhoodLayerFactory(
    [KeyFilter(KeyFilters.Neighborhoods)] Meta<ILayer>[] layers) : INeighborhoodLayerFactory
{
    private readonly Dictionary<Neighborhoods, ILayer> layers 
        = layers.ToDictionary(x => (Neighborhoods)x.Metadata[MetadataKeys.Neighborhoods], x => x.Value);

    public IReadOnlyCollection<Neighborhoods> Allowed => layers.Keys;

    public ILayer CreateNeighborhoodLayer(Neighborhoods neighborhoods)
    {
        if (layers.TryGetValue(neighborhoods, out var value))
        {
            return value;
        }

        throw new KeyNotFoundException($"{neighborhoods} was not found");
    }
}