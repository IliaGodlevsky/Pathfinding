using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Services;
using Pathfinding.Infrastructure.Data.LiteDb;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface.Models.Serialization;
using Pathfinding.Service.Interface.Models.Undefined;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Tests;

[Category("Integration")]
internal sealed class ServiceIntegrationTests
{
    [Test]
    public async Task CreatePathfindingHistory_WithRangeAndRuns_ShouldRoundTripCriticalData()
    {
        var factory = new LiteDbInMemoryUnitOfWorkFactory();
        var graphService = new GraphRequestService<FakeVertex>(factory);
        var rangeService = new RangeRequestService<FakeVertex>(factory);
        var statisticsService = new StatisticsRequestService(factory);

        var graph = await graphService.CreateGraphAsync(new CreateGraphRequest<FakeVertex>
        {
            Name = "critical-grid",
            Neighborhood = Neighborhoods.Moore,
            SmoothLevel = SmoothLevels.Low,
            Status = GraphStatuses.Editable,
            Graph = new Graph<FakeVertex>(
            [
                new() { Position = new Coordinate(0, 0), Cost = new VertexCost(1) },
                new() { Position = new Coordinate(0, 1), Cost = new VertexCost(5), IsObstacle = true },
                new() { Position = new Coordinate(1, 0), Cost = new VertexCost(2) },
                new() { Position = new Coordinate(1, 1), Cost = new VertexCost(3) }
            ],
            [2, 2])
        });

        var source = graph.Vertices.Single(v => v.Position == new Coordinate(0, 0));
        var transit = graph.Vertices.Single(v => v.Position == new Coordinate(1, 0));
        var target = graph.Vertices.Single(v => v.Position == new Coordinate(1, 1));

        await rangeService.CreatePathfindingVertexAsync(new(graph.Id, source.Id, 0));
        await rangeService.CreatePathfindingVertexAsync(new(graph.Id, target.Id, 1));
        await rangeService.CreatePathfindingVertexAsync(new(graph.Id, transit.Id, 1));

        var stats = await statisticsService.CreateStatisticsAsync(
        [
            new()
            {
                GraphId = graph.Id,
                Algorithm = Algorithms.AStar,
                Heuristics = Heuristics.Manhattan,
                StepRule = StepRules.Landscape,
                Weight = 2,
                Visited = 8,
                Steps = 5,
                Cost = 11,
                Elapsed = TimeSpan.FromMilliseconds(25),
                ResultStatus = RunStatuses.Success
            }
        ]);

        var histories = await graphService.ReadSerializationHistoriesAsync([graph.Id]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(histories.Histories, Has.Count.EqualTo(1));
            var history = histories.Histories.Single();
            Assert.That(history.Graph.Name, Is.EqualTo("critical-grid"));
            Assert.That(history.Vertices.Single(v => v.Position.Coordinate.SequenceEqual([0, 1])).IsObstacle, Is.True);
            Assert.That(
                history.Range.Select(v => v.Coordinate.ToArray()),
                Is.EqualTo(new[]
                {
                    source.Position.ToArray(),
                    transit.Position.ToArray(),
                    target.Position.ToArray()
                }));
            Assert.That(history.Statistics, Has.Count.EqualTo(1));
            Assert.That(history.Statistics[0].Id, Is.EqualTo(stats.Single().Id));
            Assert.That(history.Statistics[0].Heuristics, Is.EqualTo(Heuristics.Manhattan));
        }
    }

    [Test]
    public async Task CreatePathfindingHistoriesAsync_WhenRangeContainsOutOfBoundsVertex_ShouldRollbackWholeBatch()
    {
        var factory = new LiteDbInMemoryUnitOfWorkFactory();
        var graphService = new GraphRequestService<FakeVertex>(factory);
        var graphInfoService = new GraphInfoRequestService(factory);

        var validHistory = new PathfindingHistorySerializationModel
        {
            Graph = new GraphSerializationModel
            {
                Name = "first",
                Neighborhood = Neighborhoods.Moore,
                SmoothLevel = SmoothLevels.Low,
                Status = GraphStatuses.Editable,
                DimensionSizes = [2, 2]
            },
            Vertices =
            [
                new() { Position = new CoordinateModel { Coordinate = [0, 0] }, Cost = new VertexCostModel { Cost = 1 } },
                new() { Position = new CoordinateModel { Coordinate = [0, 1] }, Cost = new VertexCostModel { Cost = 1 } },
                new() { Position = new CoordinateModel { Coordinate = [1, 0] }, Cost = new VertexCostModel { Cost = 1 } },
                new() { Position = new CoordinateModel { Coordinate = [1, 1] }, Cost = new VertexCostModel { Cost = 1 } }
            ],
            Statistics = [],
            Range = [new() { Coordinate = [0, 0] }, new() { Coordinate = [1, 1] }]
        };

        var brokenHistory = new PathfindingHistorySerializationModel
        {
            Graph = new GraphSerializationModel
            {
                Name = "second",
                Neighborhood = Neighborhoods.Moore,
                SmoothLevel = SmoothLevels.Low,
                Status = GraphStatuses.Editable,
                DimensionSizes = [2, 2]
            },
            Vertices = validHistory.Vertices,
            Statistics = [],
            Range = [new() { Coordinate = [0, 0] }, new() { Coordinate = [99, 99] }]
        };

        Assert.ThrowsAsync<KeyNotFoundException>(() =>
            graphService.CreatePathfindingHistoriesAsync([validHistory, brokenHistory]));

        var allGraphs = await graphInfoService.ReadAllGraphInfoAsync();

        Assert.That(allGraphs, Is.Empty);
    }

    [Test]
    public async Task DeleteGraph_WithRelatedRunsAndRange_ShouldCascadeDeleteAllDependentData()
    {
        var factory = new LiteDbInMemoryUnitOfWorkFactory();
        var graphService = new GraphRequestService<FakeVertex>(factory);
        var graphInfoService = new GraphInfoRequestService(factory);
        var rangeService = new RangeRequestService<FakeVertex>(factory);
        var statisticsService = new StatisticsRequestService(factory);

        var graph = await graphService.CreateGraphAsync(new CreateGraphRequest<FakeVertex>
        {
            Name = "delete-cascade",
            Neighborhood = Neighborhoods.Moore,
            SmoothLevel = SmoothLevels.Low,
            Status = GraphStatuses.Editable,
            Graph = new Graph<FakeVertex>(
            [
                new() { Position = new Coordinate(0, 0) },
                new() { Position = new Coordinate(0, 1) }
            ],
            [1, 2])
        });

        await rangeService.CreatePathfindingVertexAsync(new(graph.Id, graph.Vertices.First().Id, 0));
        await statisticsService.CreateStatisticsAsync(
        [
            new()
            {
                GraphId = graph.Id,
                Algorithm = Algorithms.Dijkstra,
                Visited = 2,
                Steps = 1,
                Cost = 1,
                Elapsed = TimeSpan.FromMilliseconds(1),
                ResultStatus = RunStatuses.Success
            }
        ]);

        var deleted = await graphInfoService.DeleteGraphsAsync([graph.Id]);

        var rangeAfterDelete = await rangeService.ReadRangeAsync(graph.Id);
        var runsAfterDelete = await statisticsService.ReadStatisticsAsync(graph.Id);
        var graphsAfterDelete = await graphInfoService.ReadAllGraphInfoAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(deleted, Is.True);
            Assert.That(rangeAfterDelete, Is.Empty);
            Assert.That(runsAfterDelete, Is.Empty);
            Assert.That(graphsAfterDelete.Any(x => x.Id == graph.Id), Is.False);
        }
    }

    [Test]
    public async Task CreatePathfindingVertexAsync_WhenIndexIsOutOfRange_ShouldNotMutateExistingRange()
    {
        var factory = new LiteDbInMemoryUnitOfWorkFactory();
        var graphService = new GraphRequestService<FakeVertex>(factory);
        var rangeService = new RangeRequestService<FakeVertex>(factory);

        var graph = await graphService.CreateGraphAsync(new CreateGraphRequest<FakeVertex>
        {
            Name = "index-guard",
            Neighborhood = Neighborhoods.Moore,
            SmoothLevel = SmoothLevels.Low,
            Graph = new Graph<FakeVertex>(
            [
                new() { Position = new Coordinate(0, 0) },
                new() { Position = new Coordinate(0, 1) }
            ],
            [1, 2])
        });

        await rangeService.CreatePathfindingVertexAsync(new(graph.Id, graph.Vertices.First().Id, 0));

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            rangeService.CreatePathfindingVertexAsync(new(graph.Id, graph.Vertices.Last().Id, 5)));

        var range = await rangeService.ReadRangeAsync(graph.Id);
        Assert.That(range.Select(x => x.VertexId), Is.EqualTo(new[] { graph.Vertices.First().Id }));
    }

    [Test]
    public async Task ReadSerializationGraphsWithRangeAsync_WhenGraphHasNoRange_ShouldFailFast()
    {
        var factory = new LiteDbInMemoryUnitOfWorkFactory();
        var graphService = new GraphRequestService<FakeVertex>(factory);

        var graph = await graphService.CreateGraphAsync(new CreateGraphRequest<FakeVertex>
        {
            Name = "missing-range",
            Neighborhood = Neighborhoods.Moore,
            SmoothLevel = SmoothLevels.Low,
            Status = GraphStatuses.Editable,
            Graph = new Graph<FakeVertex>(
            [
                new() { Position = new Coordinate(0, 0) },
                new() { Position = new Coordinate(0, 1) }
            ],
            [1, 2])
        });

        Assert.ThrowsAsync<KeyNotFoundException>(() =>
            graphService.ReadSerializationGraphsWithRangeAsync([graph.Id]));
    }

    [Test]
    public async Task DeleteRunsAsync_ShouldDeleteOnlyRequestedRuns_AndKeepRestReadable()
    {
        var factory = new LiteDbInMemoryUnitOfWorkFactory();
        var graphService = new GraphRequestService<FakeVertex>(factory);
        var statisticsService = new StatisticsRequestService(factory);

        var graph = await graphService.CreateGraphAsync(new CreateGraphRequest<FakeVertex>
        {
            Name = "partial-run-delete",
            Neighborhood = Neighborhoods.Moore,
            SmoothLevel = SmoothLevels.Low,
            Graph = new Graph<FakeVertex>(
            [
                new() { Position = new Coordinate(0, 0) },
                new() { Position = new Coordinate(0, 1) }
            ],
            [1, 2])
        });

        var runs = await statisticsService.CreateStatisticsAsync(
        [
            new()
            {
                GraphId = graph.Id,
                Algorithm = Algorithms.AStar,
                Visited = 2,
                Steps = 1,
                Cost = 1,
                Elapsed = TimeSpan.FromMilliseconds(2),
                ResultStatus = RunStatuses.Success
            },
            new()
            {
                GraphId = graph.Id,
                Algorithm = Algorithms.Dijkstra,
                Visited = 4,
                Steps = 2,
                Cost = 2,
                Elapsed = TimeSpan.FromMilliseconds(3),
                ResultStatus = RunStatuses.Failure
            }
        ]);

        var deleted = await statisticsService.DeleteRunsAsync([runs.First().Id]);
        var afterDelete = await statisticsService.ReadStatisticsAsync(graph.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(deleted, Is.True);
            Assert.That(afterDelete, Has.Count.EqualTo(1));
            Assert.That(afterDelete.Single().Id, Is.EqualTo(runs.Last().Id));
            Assert.That(afterDelete.Single().Algorithm, Is.EqualTo(Algorithms.Dijkstra));
        }
    }

    [Test]
    public async Task UpdateVerticesAsync_ShouldPersistCriticalFlagsAndCosts()
    {
        var factory = new LiteDbInMemoryUnitOfWorkFactory();
        var graphService = new GraphRequestService<FakeVertex>(factory);

        var graph = await graphService.CreateGraphAsync(new CreateGraphRequest<FakeVertex>
        {
            Name = "vertex-update",
            Neighborhood = Neighborhoods.Moore,
            SmoothLevel = SmoothLevels.Low,
            Graph = new Graph<FakeVertex>(
            [
                new() { Position = new Coordinate(0, 0), Cost = new VertexCost(1), IsObstacle = false },
                new() { Position = new Coordinate(0, 1), Cost = new VertexCost(1), IsObstacle = false }
            ],
            [1, 2])
        });

        var updatedVertices = graph.Vertices
            .Select(x => new FakeVertex
            {
                Id = x.Id,
                Position = x.Position,
                IsObstacle = x.Position == new Coordinate(0, 1),
                Cost = new VertexCost(x.Position == new Coordinate(0, 1) ? 9 : 2)
            })
            .ToArray();

        var updated = await graphService.UpdateVerticesAsync(new(graph.Id, updatedVertices));
        var readGraph = await graphService.ReadGraphAsync(graph.Id);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(updated, Is.True);
            Assert.That(readGraph.Vertices.Single(x => x.Position == new Coordinate(0, 1)).IsObstacle, Is.True);
            Assert.That(readGraph.Vertices.Single(x => x.Position == new Coordinate(0, 1)).Cost.Cost, Is.EqualTo(9));
            Assert.That(readGraph.Vertices.Single(x => x.Position == new Coordinate(0, 0)).Cost.Cost, Is.EqualTo(2));
        }
    }
}
