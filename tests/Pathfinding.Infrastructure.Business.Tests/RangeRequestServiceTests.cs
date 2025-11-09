using Autofac;
using Autofac.Extras.Moq;
using Moq;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;
using Pathfinding.Infrastructure.Business.Services;

namespace Pathfinding.Infrastructure.Business.Tests;

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
    internal static readonly long[] expected = [1L, 99L, 2L];

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
        IReadOnlyCollection<PathfindingRange> resultRange = null!;

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
            Assert.That(ordered.Select(x => x.VertexId), Is.EqualTo(expected));
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
