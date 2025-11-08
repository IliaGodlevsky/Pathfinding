using Autofac.Extras.Moq;
using Bogus;
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
internal class GraphInfoRequestServiceTests
{
    [Test]
    public async Task ReadAllGraphInfoAsync_ShouldReturnValidInfo()
    {
        var faker = new Faker<Graph>()
            .UseSeed(Environment.TickCount)
            .RuleFor(x => x.Name, x => x.Person.UserName)
            .RuleFor(x => x.Id, x => x.IndexFaker)
            .RuleFor(x => x.SmoothLevel, x => x.Random.Enum<SmoothLevels>())
            .RuleFor(x => x.Status, x => x.Random.Enum<GraphStatuses>())
            .RuleFor(x => x.Dimensions, x => $"[{x.Random.Int(20, 100)},{x.Random.Int(20, 100)}]")
            .RuleFor(x => x.Neighborhood, x => x.Random.Enum<Neighborhoods>());
        var graphs = faker.Generate(10);
        var obstaclesCount = (IReadOnlyDictionary<int, int>)graphs.ToDictionary(x => x.Id, x => 25);
        using var mock = AutoMock.GetLoose();
        mock.Mock<IGraphParametersRepository>()
            .Setup(x => x.GetAll())
            .Returns(graphs.ToAsyncEnumerable());
        mock.Mock<IGraphParametersRepository>()
            .Setup(x => x.ReadObstaclesCountAsync(
                It.IsAny<IReadOnlyCollection<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(obstaclesCount);
        var unit = UnitOfWorkMockHelper.SetupUnitOfWork(mock, u =>
        {
            u.Setup(x => x.GraphRepository)
                .Returns(mock.Container.Resolve<IGraphParametersRepository>());
        });

        var requestService = mock.Create<GraphInfoRequestService>();

        var result = await requestService.ReadAllGraphInfoAsync();

        Assert.Multiple(() =>
        {
            mock.Mock<IUnitOfWorkFactory>().Verify(x => x.CreateAsync(It.IsAny<CancellationToken>()), Times.Once());
            unit.Verify(x => x.GraphRepository, Times.Exactly(2));
            mock.Mock<IGraphParametersRepository>().Verify(x => x.GetAll(), Times.Once());
            Assert.That(result.All(x => graphs.Any(y => y.Id == x.Id)
                                        && result.First(y => y.Id == x.Id).ObstaclesCount == obstaclesCount[x.Id]));
            Assert.That(result, Has.Count.EqualTo(graphs.Count));
        });
    }

    [Test]
    public async Task ReadGraphInfoAsync_ShouldReturnGraphInfo()
    {
        using var mock = AutoMock.GetLoose();
        var graph = new Graph
        {
            Id = 7,
            Name = "test",
            Neighborhood = Neighborhoods.Eight,
            SmoothLevel = SmoothLevels.High,
            Status = GraphStatuses.ReadOnly,
            Dimensions = "[3,4]"
        };

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.GraphRepository)
                .Returns(mock.Container.Resolve<IGraphParametersRepository>());
        });

        mock.Mock<IGraphParametersRepository>()
            .Setup(x => x.ReadAsync(graph.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(graph);

        var service = mock.Create<GraphInfoRequestService>();

        var result = await service.ReadGraphInfoAsync(graph.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(graph.Id));
            Assert.That(result.Name, Is.EqualTo(graph.Name));
            Assert.That(result.Neighborhood, Is.EqualTo(graph.Neighborhood));
            Assert.That(result.SmoothLevel, Is.EqualTo(graph.SmoothLevel));
            Assert.That(result.Status, Is.EqualTo(graph.Status));
            Assert.That(result.Dimensions, Is.EquivalentTo(new[] { 3, 4 }));
            mock.Mock<IGraphParametersRepository>()
                .Verify(x => x.ReadAsync(graph.Id, It.IsAny<CancellationToken>()), Times.Once());
        });
    }

    [Test]
    public async Task UpdateGraphInfoAsync_ShouldForwardToRepository()
    {
        using var mock = AutoMock.GetLoose();
        var model = new GraphInformationModel
        {
            Id = 9,
            Name = "graph",
            Neighborhood = Neighborhoods.Four,
            SmoothLevel = SmoothLevels.Low,
            Status = GraphStatuses.Editable,
            Dimensions = new[] { 5, 6 }
        };

        var repositoryMock = mock.Mock<IGraphParametersRepository>();
        repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Graph>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.GraphRepository).Returns(repositoryMock.Object);
        });

        var service = mock.Create<GraphInfoRequestService>();

        var result = await service.UpdateGraphInfoAsync(model);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            repositoryMock.Verify(x => x.UpdateAsync(It.Is<Graph>(g =>
                g.Id == model.Id &&
                g.Name == model.Name &&
                g.Neighborhood == model.Neighborhood &&
                g.SmoothLevel == model.SmoothLevel &&
                g.Status == model.Status &&
                g.Dimensions == "[5,6]"),
                It.IsAny<CancellationToken>()), Times.Once());
        });
    }

    [Test]
    public async Task DeleteGraphsAsync_ShouldDeleteProvidedIds()
    {
        using var mock = AutoMock.GetLoose();
        var ids = new[] { 1, 2, 3 };
        var repositoryMock = mock.Mock<IGraphParametersRepository>();
        repositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.GraphRepository).Returns(repositoryMock.Object);
        });

        var service = mock.Create<GraphInfoRequestService>();

        var result = await service.DeleteGraphsAsync(ids);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            repositoryMock.Verify(x => x.DeleteAsync(It.Is<IReadOnlyCollection<int>>(collection =>
                collection.OrderBy(v => v).SequenceEqual(ids.OrderBy(v => v))), It.IsAny<CancellationToken>()), Times.Once());
        });
    }
}

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
        var graph = new Graph<FakeVertex>(vertices, new[] { 1, 2 });
        var request = new CreateGraphRequest<FakeVertex>
        {
            Graph = graph,
            Name = "graph",
            Neighborhood = Neighborhoods.Four,
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
            Neighborhood = Neighborhoods.Eight,
            Status = GraphStatuses.ReadOnly,
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
                Neighborhood = Neighborhoods.Four,
                SmoothLevel = SmoothLevels.Medium,
                Status = GraphStatuses.Editable,
                DimensionSizes = new[] { 2, 2 }
            },
            Vertices = new List<VertexSerializationModel>
            {
                new()
                {
                    Position = new CoordinateModel { Coordinate = new[] { 0, 0 } },
                    Cost = new VertexCostModel { Cost = 1, UpperValueOfRange = 9, LowerValueOfRange = 1 },
                    IsObstacle = false
                },
                new()
                {
                    Position = new CoordinateModel { Coordinate = new[] { 0, 1 } },
                    Cost = new VertexCostModel { Cost = 2, UpperValueOfRange = 9, LowerValueOfRange = 1 },
                    IsObstacle = true
                }
            },
            Statistics = new List<RunStatisticsSerializationModel>
            {
                new()
                {
                    Algorithm = Algorithms.AStar,
                    Cost = 1,
                    Steps = 2,
                    Visited = 3,
                    Elapsed = TimeSpan.FromMilliseconds(10),
                    ResultStatus = RunStatuses.Completed
                }
            },
            Range = new List<CoordinateModel>
            {
                new() { Coordinate = new[] { 0, 0 } },
                new() { Coordinate = new[] { 0, 1 } }
            }
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

        IReadOnlyCollection<PathfindingRange> createdRange = null;
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
            Neighborhood = Neighborhoods.Four,
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
            new() { GraphId = graphId, VertexId = 1, Order = 0, IsSource = true, IsTarget = false },
            new() { GraphId = graphId, VertexId = 2, Order = 1, IsSource = false, IsTarget = true }
        };
        var stats = new List<Statistics>
        {
            new() { Id = 6, GraphId = graphId, Algorithm = Algorithms.AStar, ResultStatus = RunStatuses.Completed, Steps = 2, Visited = 3, Cost = 4, Elapsed = 5 }
        };

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.GraphRepository).Returns(mock.Container.Resolve<IGraphParametersRepository>());
            unit.Setup(x => x.VerticesRepository).Returns(mock.Container.Resolve<IVerticesRepository>());
            unit.Setup(x => x.RangeRepository).Returns(mock.Container.Resolve<IRangeRepository>());
            unit.Setup(x => x.StatisticsRepository).Returns(mock.Container.Resolve<IStatisticsRepository>());
        });

        mock.Mock<IGraphParametersRepository>()
            .Setup(x => x.ReadAsync(graphId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(graphEntity);
        mock.Mock<IVerticesRepository>()
            .Setup(x => x.ReadVerticesByGraphIdAsync(graphId))
            .Returns(vertices.ToAsyncEnumerable());
        mock.Mock<IVerticesRepository>()
            .Setup(x => x.ReadAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((long id, CancellationToken _) => vertices.First(v => v.Id == id));
        mock.Mock<IRangeRepository>()
            .Setup(x => x.ReadByGraphIdAsync(graphId))
            .Returns(range.ToAsyncEnumerable());
        mock.Mock<IStatisticsRepository>()
            .Setup(x => x.ReadByGraphIdAsync(graphId))
            .Returns(stats.ToAsyncEnumerable());

        var service = mock.Create<GraphRequestService<FakeVertex>>();

        var result = await service.ReadSerializationHistoriesAsync(new[] { graphId });

        Assert.Multiple(() =>
        {
            var history = result.Histories.Single();
            Assert.That(history.Graph.Status, Is.EqualTo(GraphStatuses.Editable));
            Assert.That(history.Vertices.Count, Is.EqualTo(vertices.Count));
            Assert.That(history.Statistics.Count, Is.EqualTo(stats.Count));
            Assert.That(history.Range.Count, Is.EqualTo(range.Count));
        });
    }

    [Test]
    public async Task ReadSerializationGraphsAsync_ShouldReturnEditableGraphsWithoutExtras()
    {
        using var mock = AutoMock.GetLoose();
        var graphId = 8;
        var graphEntity = new Graph
        {
            Id = graphId,
            Name = "graph",
            SmoothLevel = SmoothLevels.High,
            Neighborhood = Neighborhoods.Eight,
            Status = GraphStatuses.ReadOnly,
            Dimensions = "[3,3]"
        };
        var vertices = new List<Vertex>
        {
            new() { Id = 1, GraphId = graphId, Coordinates = "[0,0]", Cost = 1, UpperValueRange = 9, LowerValueRange = 1 }
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
            .Returns(vertices.ToAsyncEnumerable());

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
            Neighborhood = Neighborhoods.Four,
            Status = GraphStatuses.ReadOnly,
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

[Category("Unit")]
internal class RangeRequestServiceTests
{
    [Test]
    public async Task ReadRangeAsync_ShouldReturnModels()
    {
        using var mock = AutoMock.GetLoose();
        var graphId = 4;
        var range = new List<PathfindingRange>
        {
            new() { GraphId = graphId, VertexId = 1, Order = 0, IsSource = true, IsTarget = false },
            new() { GraphId = graphId, VertexId = 2, Order = 1, IsSource = false, IsTarget = true }
        };
        var vertices = new Dictionary<long, Vertex>
        {
            [1] = new Vertex { Id = 1, Coordinates = "[0,0]" },
            [2] = new Vertex { Id = 2, Coordinates = "[0,1]" }
        };

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.RangeRepository).Returns(mock.Container.Resolve<IRangeRepository>());
            unit.Setup(x => x.VerticesRepository).Returns(mock.Container.Resolve<IVerticesRepository>());
        });

        mock.Mock<IRangeRepository>()
            .Setup(x => x.ReadByGraphIdAsync(graphId))
            .Returns(range.ToAsyncEnumerable());
        mock.Mock<IVerticesRepository>()
            .Setup(x => x.ReadAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((long id, CancellationToken _) => vertices[id]);

        var service = mock.Create<RangeRequestService<FakeVertex>>();

        var result = await service.ReadRangeAsync(graphId);

        Assert.That(result.Select(x => x.VertexId), Is.EqualTo(range.Select(r => r.VertexId)));
    }

    [Test]
    public async Task CreatePathfindingVertexAsync_ShouldInsertVertexAndReorder()
    {
        using var mock = AutoMock.GetLoose();
        var graphId = 5;
        var existingRange = new List<PathfindingRange>
        {
            new() { GraphId = graphId, VertexId = 1, Order = 0, IsSource = true, IsTarget = false },
            new() { GraphId = graphId, VertexId = 2, Order = 1, IsSource = false, IsTarget = true }
        };
        IReadOnlyCollection<PathfindingRange> resultRange = null;

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.RangeRepository).Returns(mock.Container.Resolve<IRangeRepository>());
        });

        mock.Mock<IRangeRepository>()
            .Setup(x => x.ReadByGraphIdAsync(graphId))
            .Returns(existingRange.ToAsyncEnumerable());
        mock.Mock<IRangeRepository>()
            .Setup(x => x.UpsertAsync(It.IsAny<IReadOnlyCollection<PathfindingRange>>(), It.IsAny<CancellationToken>()))
            .Callback((IReadOnlyCollection<PathfindingRange> ranges, CancellationToken _) => resultRange = ranges)
            .ReturnsAsync((IReadOnlyCollection<PathfindingRange> ranges, CancellationToken _) => ranges);

        var service = mock.Create<RangeRequestService<FakeVertex>>();

        var result = await service.CreatePathfindingVertexAsync(graphId, 99, 1);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            Assert.That(resultRange, Is.Not.Null);
            var ordered = resultRange.OrderBy(x => x.Order).ToList();
            Assert.That(ordered.Select(x => x.VertexId), Is.EqualTo(new[] { 1L, 99L, 2L }));
            Assert.That(ordered.First().IsSource, Is.True);
            Assert.That(ordered.Last().IsTarget, Is.True);
        });
    }

    [Test]
    public async Task DeleteRangeByVerticesAsync_ShouldCallRepository()
    {
        using var mock = AutoMock.GetLoose();
        var vertices = new List<FakeVertex>
        {
            new() { Id = 5 },
            new() { Id = 6 }
        };
        var repository = mock.Mock<IRangeRepository>();
        repository
            .Setup(x => x.DeleteByVerticesIdsAsync(It.IsAny<IReadOnlyCollection<long>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.RangeRepository).Returns(repository.Object);
        });

        var service = mock.Create<RangeRequestService<FakeVertex>>();

        var result = await service.DeleteRangeAsync(vertices);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            repository.Verify(x => x.DeleteByVerticesIdsAsync(It.Is<IReadOnlyCollection<long>>(ids =>
                ids.OrderBy(v => v).SequenceEqual(vertices.Select(v => v.Id).OrderBy(v => v))), It.IsAny<CancellationToken>()), Times.Once());
        });
    }

    [Test]
    public async Task DeleteRangeByGraphIdAsync_ShouldCallRepository()
    {
        using var mock = AutoMock.GetLoose();
        var repository = mock.Mock<IRangeRepository>();
        repository
            .Setup(x => x.DeleteByGraphIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.RangeRepository).Returns(repository.Object);
        });

        var service = mock.Create<RangeRequestService<FakeVertex>>();

        var result = await service.DeleteRangeAsync(4);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            repository.Verify(x => x.DeleteByGraphIdAsync(4, It.IsAny<CancellationToken>()), Times.Once());
        });
    }
}

[Category("Unit")]
internal class StatisticsRequestServiceTests
{
    [Test]
    public async Task DeleteRunsAsync_ShouldCallRepository()
    {
        using var mock = AutoMock.GetLoose();
        var repository = mock.Mock<IStatisticsRepository>();
        repository
            .Setup(x => x.DeleteByIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.StatisticsRepository).Returns(repository.Object);
        });

        var service = mock.Create<StatisticsRequestService>();
        var ids = new[] { 1, 2 };

        var result = await service.DeleteRunsAsync(ids);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            repository.Verify(x => x.DeleteByIdsAsync(It.Is<IReadOnlyCollection<int>>(collection =>
                collection.OrderBy(v => v).SequenceEqual(ids.OrderBy(v => v))), It.IsAny<CancellationToken>()), Times.Once());
        });
    }

    [Test]
    public async Task ReadStatisticAsync_ShouldReturnModel()
    {
        using var mock = AutoMock.GetLoose();
        var repository = mock.Mock<IStatisticsRepository>();
        var statistic = new Statistics
        {
            Id = 7,
            GraphId = 3,
            Algorithm = Algorithms.AStar,
            ResultStatus = RunStatuses.Completed,
            Steps = 5,
            Visited = 8,
            Cost = 9,
            Elapsed = 10
        };
        repository
            .Setup(x => x.ReadByIdAsync(statistic.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(statistic);

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.StatisticsRepository).Returns(repository.Object);
        });

        var service = mock.Create<StatisticsRequestService>();

        var result = await service.ReadStatisticAsync(statistic.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(statistic.Id));
            Assert.That(result.GraphId, Is.EqualTo(statistic.GraphId));
            repository.Verify(x => x.ReadByIdAsync(statistic.Id, It.IsAny<CancellationToken>()), Times.Once());
        });
    }

    [Test]
    public async Task ReadStatisticsByIdsAsync_ShouldReturnModels()
    {
        using var mock = AutoMock.GetLoose();
        var repository = mock.Mock<IStatisticsRepository>();
        var stats = new List<Statistics>
        {
            new() { Id = 1, GraphId = 2, Algorithm = Algorithms.AStar, ResultStatus = RunStatuses.Completed, Steps = 1, Visited = 1, Cost = 1, Elapsed = 1 }
        };
        repository
            .Setup(x => x.ReadByIdsAsync(It.IsAny<IReadOnlyCollection<int>>()))
            .Returns(stats.ToAsyncEnumerable());

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.StatisticsRepository).Returns(repository.Object);
        });

        var service = mock.Create<StatisticsRequestService>();

        var result = await service.ReadStatisticsAsync(new[] { 1 });

        Assert.That(result, Has.Count.EqualTo(stats.Count));
    }

    [Test]
    public async Task ReadStatisticsByGraphAsync_ShouldReturnModels()
    {
        using var mock = AutoMock.GetLoose();
        var repository = mock.Mock<IStatisticsRepository>();
        var stats = new List<Statistics>
        {
            new() { Id = 3, GraphId = 4, Algorithm = Algorithms.Dijkstra, ResultStatus = RunStatuses.Completed, Steps = 2, Visited = 2, Cost = 2, Elapsed = 2 }
        };
        repository
            .Setup(x => x.ReadByGraphIdAsync(4))
            .Returns(stats.ToAsyncEnumerable());

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.StatisticsRepository).Returns(repository.Object);
        });

        var service = mock.Create<StatisticsRequestService>();

        var result = await service.ReadStatisticsAsync(4);

        Assert.That(result.Single().GraphId, Is.EqualTo(4));
    }

    [Test]
    public async Task CreateStatisticsAsync_ShouldPersistAndReturnModels()
    {
        using var mock = AutoMock.GetLoose();
        var repository = mock.Mock<IStatisticsRepository>();
        repository
            .Setup(x => x.CreateAsync(It.IsAny<IReadOnlyCollection<Statistics>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Statistics> stats, CancellationToken _) => stats);

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.StatisticsRepository).Returns(repository.Object);
        });

        var request = new List<CreateStatisticsRequest>
        {
            new()
            {
                Algorithm = Algorithms.AStar,
                GraphId = 1,
                ResultStatus = RunStatuses.Completed,
                Steps = 1,
                Visited = 1,
                Cost = 1,
                Elapsed = TimeSpan.FromMilliseconds(1)
            }
        };

        var service = mock.Create<StatisticsRequestService>();

        var result = await service.CreateStatisticsAsync(request);

        Assert.That(result.Single().GraphId, Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateStatisticsAsync_ShouldForwardToRepository()
    {
        using var mock = AutoMock.GetLoose();
        var repository = mock.Mock<IStatisticsRepository>();
        repository
            .Setup(x => x.UpdateAsync(It.IsAny<IReadOnlyCollection<Statistics>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.StatisticsRepository).Returns(repository.Object);
        });

        var service = mock.Create<StatisticsRequestService>();
        var models = new List<RunStatisticsModel>
        {
            new()
            {
                Id = 1,
                GraphId = 2,
                Algorithm = Algorithms.Dijkstra,
                ResultStatus = RunStatuses.Completed,
                Steps = 2,
                Visited = 3,
                Cost = 4,
                Elapsed = TimeSpan.FromMilliseconds(5)
            }
        };

        var result = await service.UpdateStatisticsAsync(models);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.True);
            repository.Verify(x => x.UpdateAsync(It.Is<IReadOnlyCollection<Statistics>>(stats =>
                stats.Single().Id == 1 && stats.Single().GraphId == 2),
                It.IsAny<CancellationToken>()), Times.Once());
        });
    }
}

internal static class UnitOfWorkMockHelper
{
    internal static Mock<IUnitOfWork> SetupUnitOfWork(AutoMock mock, Action<Mock<IUnitOfWork>> configure)
    {
        var unit = mock.Mock<IUnitOfWork>();
        unit.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        unit.Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unit.Setup(x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        unit.Setup(x => x.DisposeAsync()).Returns(ValueTask.CompletedTask);
        configure?.Invoke(unit);
        mock.Mock<IUnitOfWorkFactory>()
            .Setup(x => x.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(unit.Object);
        return unit;
    }
}
