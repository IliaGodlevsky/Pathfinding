using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Factories;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Models.Undefined;
using Pathfinding.Shared.Primitives;
using System.Reactive.Linq;

namespace Pathfinding.App.Console.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class RunUpdateViewModelTests
{
    [Test]
    public async Task RunsSelectedMessage_ShouldEnableUpdateCommandAsync()
    {
        var messenger = new StrongReferenceMessenger();
        var graphServiceMock = new Mock<IGraphRequestService<GraphVertexModel>>();
        var rangeServiceMock = new Mock<IRangeRequestService<GraphVertexModel>>();
        var statisticsServiceMock = new Mock<IStatisticsRequestService>();
        var algorithmsFactoryMock = CreateAlgorithmsFactoryMock();
        var neighborhoodFactoryMock = new Mock<INeighborhoodLayerFactory>();

        using var viewModel = CreateViewModel(
            messenger,
            graphServiceMock,
            rangeServiceMock,
            statisticsServiceMock,
            algorithmsFactoryMock,
            neighborhoodFactoryMock);

        var graph = CreateGraph();

        messenger.Send(new GraphActivatedMessage(new ActivatedGraphModel(
            new(10, graph, false),
            Neighborhoods.Moore,
            SmoothLevels.No)));

        var runInfo = new RunInfoModel { Id = 1, Algorithm = Algorithms.AStar };
        messenger.Send(new RunsSelectedMessage([runInfo]));

        Assert.That(await viewModel.UpdateRunsCommand.CanExecute.FirstAsync(x => x), Is.True);
    }

    [Test]
    public async Task UpdateRunsCommand_ShouldRefreshStatisticsAndNotifyAsync()
    {
        var messenger = new StrongReferenceMessenger();
        var graphServiceMock = new Mock<IGraphRequestService<GraphVertexModel>>();
        var rangeServiceMock = new Mock<IRangeRequestService<GraphVertexModel>>();
        var statisticsServiceMock = new Mock<IStatisticsRequestService>();
        var algorithmsFactoryMock = CreateAlgorithmsFactoryMock();
        var neighborhoodFactoryMock = new Mock<INeighborhoodLayerFactory>();

        var graph = CreateGraph();
        var range = new[]
        {
            new PathfindingRangeModel { Position = new Coordinate(0), IsSource = true },
            new PathfindingRangeModel { Position = new Coordinate(1), IsTarget = true }
        };

        rangeServiceMock
            .Setup(x => x.ReadRangeAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(range);

        statisticsServiceMock
            .Setup(x => x.ReadStatisticsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new() { Id = 1, GraphId = 10, Algorithm = Algorithms.AStar }
            ]);

        statisticsServiceMock
            .Setup(x => x.UpdateStatisticsAsync(
                It.IsAny<IEnumerable<RunStatisticsModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        using var viewModel = CreateViewModel(
            messenger,
            graphServiceMock,
            rangeServiceMock,
            statisticsServiceMock,
            algorithmsFactoryMock,
            neighborhoodFactoryMock);

        messenger.Send(new GraphActivatedMessage(new ActivatedGraphModel(
            new(10, graph, false),
            default,
            default)));

        var runInfo = new RunInfoModel { Id = 1, Algorithm = Algorithms.AStar };
        messenger.Send(new RunsSelectedMessage([runInfo]));

        RunsUpdatedMessage updatedMessage = null;
        messenger.Register<RunsUpdatedMessage>(this, (_, msg) => updatedMessage = msg);

        await viewModel.UpdateRunsCommand.Execute();

        Assert.Multiple(() =>
        {
            statisticsServiceMock.Verify(x => x.ReadStatisticsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            statisticsServiceMock.Verify(x => x.UpdateStatisticsAsync(
                It.IsAny<IEnumerable<RunStatisticsModel>>(),
                It.IsAny<CancellationToken>()), Times.Once);
            Assert.That(updatedMessage, Is.Not.Null);
        });
    }

    [Test]
    public async Task GraphsDeletedMessage_ShouldClearSelectionAsync()
    {
        var messenger = new StrongReferenceMessenger();
        var graphServiceMock = new Mock<IGraphRequestService<GraphVertexModel>>();
        var rangeServiceMock = new Mock<IRangeRequestService<GraphVertexModel>>();
        var statisticsServiceMock = new Mock<IStatisticsRequestService>();
        var algorithmsFactoryMock = CreateAlgorithmsFactoryMock();
        var neighborhoodFactoryMock = new Mock<INeighborhoodLayerFactory>();

        using var viewModel = CreateViewModel(
            messenger,
            graphServiceMock,
            rangeServiceMock,
            statisticsServiceMock,
            algorithmsFactoryMock,
            neighborhoodFactoryMock);

        var graph = CreateGraph();
        messenger.Send(new GraphActivatedMessage(new ActivatedGraphModel(
            new(5, graph, false),
            default,
            default)));

        messenger.Send(new RunsSelectedMessage([
            new RunInfoModel { Id = 1, Algorithm = Algorithms.AStar }
        ]));

        messenger.Send(new GraphsDeletedMessage([5]));

        Assert.That(await viewModel.UpdateRunsCommand.CanExecute.FirstAsync(), Is.False);
    }

    private static RunUpdateViewModel CreateViewModel(
        IMessenger messenger,
        Mock<IGraphRequestService<GraphVertexModel>> graphServiceMock,
        Mock<IRangeRequestService<GraphVertexModel>> rangeServiceMock,
        Mock<IStatisticsRequestService> statisticsServiceMock,
        Mock<IAlgorithmsFactory> algorithmsFactoryMock,
        Mock<INeighborhoodLayerFactory> neighborhoodFactoryMock,
        ILog log = null)
    {
        return new RunUpdateViewModel(
            graphServiceMock.Object,
            rangeServiceMock.Object,
            statisticsServiceMock.Object,
            algorithmsFactoryMock.Object,
            neighborhoodFactoryMock.Object,
            messenger,
            log ?? Mock.Of<ILog>());
    }

    private static Mock<IAlgorithmsFactory> CreateAlgorithmsFactoryMock()
    {
        var factory = new TestAlgorithmFactory();
        var algorithmsFactoryMock = new Mock<IAlgorithmsFactory>();
        algorithmsFactoryMock.SetupGet(x => x.Allowed).Returns([Algorithms.AStar]);
        algorithmsFactoryMock
            .Setup(x => x.GetAlgorithmFactory(It.IsAny<Algorithms>()))
            .Returns(factory);
        return algorithmsFactoryMock;
    }

    private static Graph<GraphVertexModel> CreateGraph()
    {
        var first = new GraphVertexModel { Position = new Coordinate(0) };
        var second = new GraphVertexModel { Position = new Coordinate(1) };
        first.Neighbors.Add(second);
        second.Neighbors.Add(first);
        return new Graph<GraphVertexModel>([first, second], [2]);
    }
}
