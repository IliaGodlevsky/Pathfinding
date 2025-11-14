using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Factories;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Messages.ViewModel.Requests;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Undefined;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Shared.Primitives;
using System.Reactive.Linq;

namespace Pathfinding.App.Console.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class RunCreateViewModelTests
{
    [Test]
    public async Task GraphLifecycle_ShouldToggleCommandAvailabilityAsync()
    {
        var messenger = new StrongReferenceMessenger();
        var statisticsServiceMock = new Mock<IStatisticsRequestService>();
        var algorithmsFactoryMock = CreateAlgorithmsFactoryMock();
        var heuristicsFactoryMock = new Mock<IHeuristicsFactory>();
        heuristicsFactoryMock.SetupGet(x => x.Allowed).Returns([]);
        var stepRuleFactoryMock = new Mock<IStepRuleFactory>();
        stepRuleFactoryMock.SetupGet(x => x.Allowed).Returns([]);

        using var viewModel = CreateViewModel(
            messenger,
            statisticsServiceMock,
            algorithmsFactoryMock,
            heuristicsFactoryMock,
            stepRuleFactoryMock);

        var graph = CreateGraph();
        var message = new GraphActivatedMessage(new ActivatedGraphModel(
            new(42, graph, false),
            default,
            default));
        messenger.Send(message);

        viewModel.SelectedAlgorithms.Add(Algorithms.AStar);

        Assert.That(await viewModel.CreateRunCommand.CanExecute.FirstAsync(), Is.True);

        messenger.Send(new GraphsDeletedMessage([42]));

        Assert.That(await viewModel.CreateRunCommand.CanExecute.FirstAsync(), Is.False);
    }

    [Test]
    public async Task CreateRunCommand_ShouldRequestStatisticsAndPublishMessageAsync()
    {
        var messenger = new StrongReferenceMessenger();
        var statisticsServiceMock = new Mock<IStatisticsRequestService>();
        var algorithmsFactoryMock = CreateAlgorithmsFactoryMock();
        var heuristicsFactoryMock = new Mock<IHeuristicsFactory>();
        heuristicsFactoryMock.SetupGet(x => x.Allowed).Returns([]);
        var stepRuleFactoryMock = new Mock<IStepRuleFactory>();
        stepRuleFactoryMock.SetupGet(x => x.Allowed).Returns([]);
        var logMock = new Mock<ILog>();

        var createdRuns = new[]
        {
            new RunStatisticsModel { Id = 1, GraphId = 42, Algorithm = Algorithms.AStar }
        };

        statisticsServiceMock
            .Setup(x => x.CreateStatisticsAsync(
                It.IsAny<IEnumerable<CreateStatisticsRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdRuns);

        using var viewModel = CreateViewModel(
            messenger,
            statisticsServiceMock,
            algorithmsFactoryMock,
            heuristicsFactoryMock,
            stepRuleFactoryMock,
            logMock.Object);

        var graph = CreateGraph();
        var message = new GraphActivatedMessage(new ActivatedGraphModel(
            new(42, graph, false),
            default,
            default));
        messenger.Send(message);

        messenger.Register<PathfindingRangeRequestMessage>(this, (_, msg)
            => msg.Reply([.. graph.Select(v => v)]));

        viewModel.SelectedAlgorithms.Add(Algorithms.AStar);

        RunsCreatedMessaged createdMessage = null;
        messenger.Register<RunsCreatedMessaged>(this, (_, msg) => createdMessage = msg);

        await viewModel.CreateRunCommand.Execute();

        Assert.Multiple(() =>
        {
            statisticsServiceMock.Verify(x => x.CreateStatisticsAsync(
                It.Is<IEnumerable<CreateStatisticsRequest>>(requests => requests.Any()),
                It.IsAny<CancellationToken>()), Times.Once);
            Assert.That(createdMessage, Is.Not.Null);
        });
    }

    [Test]
    public async Task CreateRunCommand_RangeTooSmall_ShouldLogInformationAsync()
    {
        var messenger = new StrongReferenceMessenger();
        var statisticsServiceMock = new Mock<IStatisticsRequestService>();
        var algorithmsFactoryMock = CreateAlgorithmsFactoryMock();
        var heuristicsFactoryMock = new Mock<IHeuristicsFactory>();
        heuristicsFactoryMock.SetupGet(x => x.Allowed).Returns([]);
        var stepRuleFactoryMock = new Mock<IStepRuleFactory>();
        stepRuleFactoryMock.SetupGet(x => x.Allowed).Returns([]);
        var logMock = new Mock<ILog>();

        using var viewModel = CreateViewModel(
            messenger,
            statisticsServiceMock,
            algorithmsFactoryMock,
            heuristicsFactoryMock,
            stepRuleFactoryMock,
            logMock.Object);

        var graph = CreateGraph();

        var message = new GraphActivatedMessage(new ActivatedGraphModel(
            new(42, graph, false),
            default,
            default));
        messenger.Send(message);

        messenger.Register<PathfindingRangeRequestMessage>(this, (_, msg)
            => msg.Reply([graph.First()]));

        viewModel.SelectedAlgorithms.Add(Algorithms.AStar);

        await viewModel.CreateRunCommand.Execute();

        statisticsServiceMock.Verify(x => x.CreateStatisticsAsync(
            It.IsAny<IEnumerable<CreateStatisticsRequest>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        logMock.Verify(x => x.Info(It.IsAny<string>()), Times.Once);
    }

    private static RunCreateViewModel CreateViewModel(
        IMessenger messenger,
        Mock<IStatisticsRequestService> statisticsServiceMock,
        Mock<IAlgorithmsFactory> algorithmsFactoryMock,
        Mock<IHeuristicsFactory> heuristicsFactoryMock,
        Mock<IStepRuleFactory> stepRuleFactoryMock,
        ILog log = null)
    {
        return new RunCreateViewModel(
            statisticsServiceMock.Object,
            algorithmsFactoryMock.Object,
            heuristicsFactoryMock.Object,
            stepRuleFactoryMock.Object,
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
