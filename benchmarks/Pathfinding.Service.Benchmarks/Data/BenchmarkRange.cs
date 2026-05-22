using Pathfinding.Data;
using Pathfinding.Data.Extensions;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Layers;
using Pathfinding.Shared.Primitives;
using System.Collections;

namespace Pathfinding.Service.Benchmarks.Data;

internal sealed class BenchmarkRange :
    Singleton<BenchmarkRange, IEnumerable<BenchmarkVertex>>,
    IEnumerable<BenchmarkVertex>
{
    private static readonly IGraph<BenchmarkVertex> graph;

    private BenchmarkRange()
    {

    }

    static BenchmarkRange()
    {
        var assemble = new GraphAssemble<BenchmarkVertex>();
        var neighborhoodLayer = new VonNeumannNeighborhoodLayer();
        var costLayer = new VertexCostLayer(
            range => new VertexCost(Random.Shared.Next(range.LowerValueOfRange,
                range.UpperValueOfRange + 1)));
        graph = assemble.AssembleGraph([200, 250]);
        graph.CostRange = new InclusiveValueRange<int>(1, 9);
        var layers = new Layers.Layers(neighborhoodLayer, costLayer);
        layers.Overlay(graph);
    }

    public IEnumerator<BenchmarkVertex> GetEnumerator()
    {
        yield return graph.Get(10, 10);
        yield return graph.Get(120, 30);
        yield return graph.Get(30, 100);
        yield return graph.Get(10, 110);
        yield return graph.Get(150, 110);
        yield return graph.Get(180, 180);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}