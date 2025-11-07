using Autofac;
using Autofac.Extras.Moq;
using Bogus;
using Moq;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Domain.Interface.Repositories;
using Pathfinding.Infrastructure.Business;

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
            .Returns(Task.FromResult(obstaclesCount));
        mock.Mock<IUnitOfWork>()
            .Setup(x => x.GraphRepository)
            .Returns(mock.Container.Resolve<IGraphParametersRepository>());
        mock.Mock<IUnitOfWorkFactory>()
            .Setup(x => x.CreateAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(mock.Container.Resolve<IUnitOfWork>()));

        var requestService = mock.Create<GraphInfoRequestService>();

        var result = await requestService.ReadAllGraphInfoAsync();

        Assert.Multiple(() =>
        {
            mock.Mock<IUnitOfWorkFactory>().Verify(x => x.CreateAsync(It.IsAny<CancellationToken>()), Times.Once());
            mock.Mock<IUnitOfWork>().Verify(x => x.GraphRepository, Times.Exactly(2));
            mock.Mock<IGraphParametersRepository>().Verify(x => x.GetAll(), Times.Once());
            Assert.That(result.All(x => graphs.Any(y => y.Id == x.Id)
                                        && result.First(y => y.Id == x.Id).ObstaclesCount == obstaclesCount[x.Id]));
            Assert.That(result, Has.Count.EqualTo(graphs.Count));
        });
    }
}