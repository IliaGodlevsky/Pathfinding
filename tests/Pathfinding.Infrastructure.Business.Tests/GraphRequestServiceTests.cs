using Autofac.Extras.Moq;
using Moq;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Domain.Interface.Repositories;
using Pathfinding.Infrastructure.Business.Services;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Models.Serialization;
using Pathfinding.Service.Interface.Models.Undefined;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Service.Interface.Requests.Update;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Tests;

[Category("Unit")]
internal class GraphRequestServiceTests
{
    [Test]
    public async Task CreateGraphAsync_ShouldPersistGraphAndVertices()
    {
        using var mock = AutoMock.GetLoose();
        var vertices = new List<FakeVertex>
        {
            new() { Position = new Coordinate(0, 0) },
            new() { Position = new Coordinate(0, 1) }
        };
        var graph = new Graph<FakeVertex>(vertices, [1, 2]);
        var request = new CreateGraphRequest<FakeVertex>
        {
            Graph = graph,
            Name = "graph",
            Neighborhood = Neighborhoods.Moore,
            SmoothLevel = SmoothLevels.Low,
            Status = GraphStatuses.Editable
        };

        var repository = mock.Mock<IGraphParametersRepository>();
        repository
            .Setup(x => x.CreateAsync(It.IsAny<Graph>(), It.IsAny<CancellationToken>()))
            .Callback((Graph g, CancellationToken _) => g.Id = 11)
            .ReturnsAsync((Graph g, CancellationToken _) => g);

        var verticesRepository = mock.Mock<IVerticesRepository>();
        verticesRepository
            .Setup(x => x.CreateAsync(It.IsAny<IReadOnlyCollection<Vertex>>(), It.IsAny<CancellationToken>()))
            .Callback((IReadOnlyCollection<Vertex> created, CancellationToken _) =>
            {
                long id = 1;
                foreach (var vertex in created)
                {
                    vertex.Id = id++;
                }
            })
            .ReturnsAsync((IReadOnlyCollection<Vertex> created, CancellationToken _) => created);

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.GraphRepository).Returns(repository.Object);
            unit.Setup(x => x.VerticesRepository).Returns(verticesRepository.Object);
        });

        var service = mock.Create<GraphRequestService<FakeVertex>>();

        var result = await service.CreateGraphAsync(request);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(11));
            Assert.That(result.DimensionSizes, Is.EqualTo(graph.DimensionsSizes));
            Assert.That(result.Vertices.Select(v => v.Id), Is.EqualTo(new[] { 1L, 2L }));
            repository.Verify(x => x.CreateAsync(It.IsAny<Graph>(), It.IsAny<CancellationToken>()), Times.Once());
            verticesRepository.Verify(x => x.CreateAsync(It.IsAny<IReadOnlyCollection<Vertex>>(), It.IsAny<CancellationToken>()), Times.Once());
        });
    }

    [Test]
    public async Task ReadGraphAsync_ShouldReturnGraphModel()
    {
        using var mock = AutoMock.GetLoose();
        var graphId = 5;
        var graphEntity = new Graph
        {
            Id = graphId,
            Name = "graph",
            SmoothLevel = SmoothLevels.Low,
            Neighborhood = Neighborhoods.Moore,
            Status = GraphStatuses.Readonly,
            Dimensions = "[2,2]"
        };
        var vertexEntities = new List<Vertex>
        {
            new() { Id = 10, GraphId = graphId, Coordinates = "[0,0]", Cost = 1, UpperValueRange = 9, LowerValueRange = 1 },
            new() { Id = 11, GraphId = graphId, Coordinates = "[0,1]", Cost = 1, UpperValueRange = 9, LowerValueRange = 1 }
        };

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.GraphRepository).Returns(mock.Container.Resolve<IGraphParametersRepository>());
            unit.Setup(x => x.VerticesRepository).Returns(mock.Container.Resolve<IVerticesRepository>());
        });

        mock.Mock<IGraphParametersRepository>()
            .Setup(x => x.ReadAsync(graphId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(graphEntity);
        mock.Mock<IVerticesRepository>()
            .Setup(x => x.ReadVerticesByGraphIdAsync(graphId))
            .Returns(vertexEntities.ToAsyncEnumerable());

        var service = mock.Create<GraphRequestService<FakeVertex>>();

        var result = await service.ReadGraphAsync(graphId);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(graphId));
            Assert.That(result.Name, Is.EqualTo(graphEntity.Name));
            Assert.That(result.Vertices.Select(x => x.Id), Is.EqualTo(vertexEntities.Select(v => v.Id)));
            Assert.That(result.DimensionSizes, Is.EqualTo(new[] { 2, 2 }));
        });
    }

    [Test]
    public async Task UpdateVerticesAsync_ShouldUpdateRepository()
    {
        using var mock = AutoMock.GetLoose();
        var vertices = new List<FakeVertex>
        {
            new() { Id = 2, Position = new Coordinate(0, 0) }
        };
        var request = new UpdateVerticesRequest<FakeVertex>(3, vertices);

        var repository = mock.Mock<IVerticesRepository>();
        repository
            .Setup(x => x.UpdateVerticesAsync(It.IsAny<IReadOnlyCollection<Vertex>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.VerticesRepository).Returns(repository.Object);
        });

        var service = mock.Create<GraphRequestService<FakeVertex>>();

        var result = await service.UpdateVerticesAsync(request);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            repository.Verify(x => x.UpdateVerticesAsync(It.Is<IReadOnlyCollection<Vertex>>(collection =>
                collection.All(v => v.GraphId == request.GraphId)), It.IsAny<CancellationToken>()), Times.Once());
        });
    }

    [Test]
    public async Task CreatePathfindingHistoriesAsync_ShouldCreateAllEntities()
    {
        using var mock = AutoMock.GetLoose();
        var history = new PathfindingHistorySerializationModel
        {
            Graph = new GraphSerializationModel
            {
                Name = "graph",
                Neighborhood = Neighborhoods.Moore,
                SmoothLevel = SmoothLevels.Medium,
                Status = GraphStatuses.Editable,
                DimensionSizes = [2, 2]
            },
            Vertices =
            [
                new()
                {
                    Position = new CoordinateModel { Coordinate = [0, 0] },
                    Cost = new VertexCostModel { Cost = 1, UpperValueOfRange = 9, LowerValueOfRange = 1 },
                    IsObstacle = false
                },
                new()
                {
                    Position = new CoordinateModel { Coordinate = [0, 1] },
                    Cost = new VertexCostModel { Cost = 2, UpperValueOfRange = 9, LowerValueOfRange = 1 },
                    IsObstacle = true
                }
            ],
            Statistics =
            [
                new()
                {
                    Algorithm = Domain.Core.Enums.Algorithms.AStar,
                    Cost = 1,
                    Steps = 2,
                    Visited = 3,
                    Elapsed = TimeSpan.FromMilliseconds(10),
                    ResultStatus = RunStatuses.Success
                }
            ],
            Range =
            [
                new() { Coordinate = [0, 0] },
                new() { Coordinate = [0, 1] }
            ]
        };

        var graphRepository = mock.Mock<IGraphParametersRepository>();
        graphRepository
            .Setup(x => x.CreateAsync(It.IsAny<Graph>(), It.IsAny<CancellationToken>()))
            .Callback((Graph g, CancellationToken _) => g.Id = 42)
            .ReturnsAsync((Graph g, CancellationToken _) => g);

        var verticesRepository = mock.Mock<IVerticesRepository>();
        verticesRepository
            .Setup(x => x.CreateAsync(It.IsAny<IReadOnlyCollection<Vertex>>(), It.IsAny<CancellationToken>()))
            .Callback((IReadOnlyCollection<Vertex> created, CancellationToken _) =>
            {
                long id = 1;
                foreach (var vertex in created)
                {
                    vertex.Id = id++;
                }
            })
            .ReturnsAsync((IReadOnlyCollection<Vertex> created, CancellationToken _) => created);

        var statisticsRepository = mock.Mock<IStatisticsRepository>();
        statisticsRepository
            .Setup(x => x.CreateAsync(It.IsAny<IReadOnlyCollection<Statistics>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Statistics> created, CancellationToken _) => created);

        IReadOnlyCollection<PathfindingRange> createdRange = null!;
        var rangeRepository = mock.Mock<IRangeRepository>();
        rangeRepository
            .Setup(x => x.CreateAsync(It.IsAny<IReadOnlyCollection<PathfindingRange>>(), It.IsAny<CancellationToken>()))
            .Callback((IReadOnlyCollection<PathfindingRange> ranges, CancellationToken _) => createdRange = ranges)
            .ReturnsAsync((IReadOnlyCollection<PathfindingRange> ranges, CancellationToken _) => ranges);

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.GraphRepository).Returns(graphRepository.Object);
            unit.Setup(x => x.VerticesRepository).Returns(verticesRepository.Object);
            unit.Setup(x => x.StatisticsRepository).Returns(statisticsRepository.Object);
            unit.Setup(x => x.RangeRepository).Returns(rangeRepository.Object);
        });

        var service = mock.Create<GraphRequestService<FakeVertex>>();

        var result = await service.CreatePathfindingHistoriesAsync(new[] { history });

        Assert.Multiple(() =>
        {
            Assert.That(result, Has.Count.EqualTo(1));
            var created = result.Single();
            Assert.That(created.Graph.Id, Is.EqualTo(42));
            Assert.That(created.Graph.Vertices.Select(v => v.Id), Is.EqualTo(new[] { 1L, 2L }));
            Assert.That(created.Statistics.Single().GraphId, Is.EqualTo(42));
            Assert.That(created.Range.Select(c => c.ToString()), Is.EqualTo(new[] { "(0,0)", "(0,1)" }));
            Assert.That(createdRange, Is.Not.Null);
            Assert.That(createdRange.Select(r => r.VertexId), Is.EqualTo(new[] { 1L, 2L }));
        });
    }

    [Test]
    public async Task ReadSerializationHistoriesAsync_ShouldReturnFullHistory()
    {
        using var mock = AutoMock.GetLoose();
        var graphId = 3;
        var graphEntity = new Graph
        {
            Id = graphId,
            Name = "graph",
            SmoothLevel = SmoothLevels.Low,
            Neighborhood = Neighborhoods.Moore,
            Status = GraphStatuses.Editable,
            Dimensions = "[2,2]"
        };
        var vertices = new List<Vertex>
        {
            new() { Id = 1, GraphId = graphId, Coordinates = "[0,0]", Cost = 1, UpperValueRange = 9, LowerValueRange = 1 },
            new() { Id = 2, GraphId = graphId, Coordinates = "[0,1]", Cost = 2, UpperValueRange = 9, LowerValueRange = 1 }
        };
        var range = new List<PathfindingRange>
        {
            new() { GraphId = graphId, VertexId = 1, Order = 0, IsSource = true, IsTarget = false }
        };

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.GraphRepository).Returns(mock.Container.Resolve<IGraphParametersRepository>());
            unit.Setup(x => x.VerticesRepository).Returns(mock.Container.Resolve<IVerticesRepository>());
            unit.Setup(x => x.RangeRepository).Returns(mock.Container.Resolve<IRangeRepository>());
        });

        mock.Mock<IGraphParametersRepository>()
            .Setup(x => x.ReadAsync(graphId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(graphEntity);
        mock.Mock<IVerticesRepository>()
            .Setup(x => x.ReadVerticesByGraphIdAsync(graphId))
            .Returns(vertices.ToAsyncEnumerable());
        mock.Mock<IRangeRepository>()
            .Setup(x => x.ReadByGraphIdAsync(graphId))
            .Returns(range.ToAsyncEnumerable());

        var service = mock.Create<GraphRequestService<FakeVertex>>();

        var result = await service.ReadSerializationGraphsAsync(new[] { graphId });

        Assert.Multiple(() =>
        {
            var history = result.Histories.Single();
            Assert.That(history.Graph.Status, Is.EqualTo(GraphStatuses.Editable));
            Assert.That(history.Statistics, Is.Empty);
            Assert.That(history.Range, Is.Empty);
        });
    }

    [Test]
    public async Task ReadSerializationGraphsWithRangeAsync_ShouldIncludeRange()
    {
        using var mock = AutoMock.GetLoose();
        var graphId = 9;
        var graphEntity = new Graph
        {
            Id = graphId,
            Name = "graph",
            SmoothLevel = SmoothLevels.Low,
            Neighborhood = Neighborhoods.Moore,
            Status = GraphStatuses.Readonly,
            Dimensions = "[2,2]"
        };
        var vertices = new List<Vertex>
        {
            new() { Id = 1, GraphId = graphId, Coordinates = "[0,0]", Cost = 1, UpperValueRange = 9, LowerValueRange = 1 }
        };
        var range = new List<PathfindingRange>
        {
            new() { GraphId = graphId, VertexId = 1, Order = 0, IsSource = true, IsTarget = true }
        };

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.GraphRepository).Returns(mock.Container.Resolve<IGraphParametersRepository>());
            unit.Setup(x => x.VerticesRepository).Returns(mock.Container.Resolve<IVerticesRepository>());
            unit.Setup(x => x.RangeRepository).Returns(mock.Container.Resolve<IRangeRepository>());
        });

        mock.Mock<IGraphParametersRepository>()
            .Setup(x => x.ReadAsync(graphId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(graphEntity);
        mock.Mock<IVerticesRepository>()
            .Setup(x => x.ReadVerticesByGraphIdAsync(graphId))
            .Returns(vertices.ToAsyncEnumerable());
        mock.Mock<IRangeRepository>()
            .Setup(x => x.ReadByGraphIdAsync(graphId))
            .Returns(range.ToAsyncEnumerable());
        mock.Mock<IVerticesRepository>()
            .Setup(x => x.ReadAsync(range[0].VertexId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vertices[0]);

        var service = mock.Create<GraphRequestService<FakeVertex>>();

        var result = await service.ReadSerializationGraphsWithRangeAsync(new[] { graphId });

        Assert.Multiple(() =>
        {
            var history = result.Histories.Single();
            Assert.That(history.Graph.Status, Is.EqualTo(GraphStatuses.Editable));
            Assert.That(history.Range, Has.Count.EqualTo(1));
            Assert.That(history.Statistics, Is.Empty);
        });
    }
}
