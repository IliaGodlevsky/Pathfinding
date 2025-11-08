using Autofac.Extras.Moq;
using Moq;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Domain.Interface.Repositories;
using Pathfinding.Infrastructure.Business.Services;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Requests.Create;

namespace Pathfinding.Infrastructure.Business.Tests;

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
        var entity = new Statistics
        {
            Id = 3,
            GraphId = 4,
            Algorithm = Domain.Core.Enums.Algorithms.AStar,
            ResultStatus = RunStatuses.Success,
            Steps = 2,
            Visited = 2,
            Cost = 2,
            Elapsed = 2
        };
        repository
            .Setup(x => x.ReadAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        UnitOfWorkMockHelper.SetupUnitOfWork(mock, unit =>
        {
            unit.Setup(x => x.StatisticsRepository).Returns(repository.Object);
        });

        var service = mock.Create<StatisticsRequestService>();

        var result = await service.ReadStatisticAsync(3);

        Assert.Multiple(() =>
        {
            Assert.That(result.Id, Is.EqualTo(3));
            repository.Verify(x => x.ReadAsync(3, It.IsAny<CancellationToken>()), Times.Once());
        });
    }

    [Test]
    public async Task ReadStatisticsByGraphAsync_ShouldReturnModels()
    {
        using var mock = AutoMock.GetLoose();
        var repository = mock.Mock<IStatisticsRepository>();
        var stats = new List<Statistics>
        {
            new() { Id = 3, GraphId = 4, Algorithm = Domain.Core.Enums.Algorithms.AStar, ResultStatus = RunStatuses.Success, Steps = 2, Visited = 2, Cost = 2, Elapsed = 2 }
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
                Algorithm = Domain.Core.Enums.Algorithms.AStar,
                GraphId = 1,
                ResultStatus = RunStatuses.Success,
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
                Algorithm = Domain.Core.Enums.Algorithms.AStar,
                ResultStatus = RunStatuses.Success,
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
