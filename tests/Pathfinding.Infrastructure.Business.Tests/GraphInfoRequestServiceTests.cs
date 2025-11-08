using Autofac;
using Autofac.Extras.Moq;
using Bogus;
using Moq;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Domain.Interface.Repositories;
using Pathfinding.Infrastructure.Business.Services;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Requests.Update;

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
    internal static readonly int[] expected = [3, 4];

    [Test]
    public async Task ReadGraphInfoAsync_ShouldReturnGraphInfo()
    {
        using var mock = AutoMock.GetLoose();
        var graph = new Graph
        {
            Id = 7,
            Name = "test",
            Neighborhood = Neighborhoods.Moore,
            SmoothLevel = SmoothLevels.High,
            Status = GraphStatuses.Readonly,
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
            Assert.That(result.Dimensions, Is.EquivalentTo(expected));
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
            Neighborhood = Neighborhoods.Moore,
            SmoothLevel = SmoothLevels.Low,
            Status = GraphStatuses.Editable,
            Dimensions = [5, 6]
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
