using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Models.Undefined;
using System.Reactive.Linq;

namespace Pathfinding.App.Console.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class RunTableViewModelTests
{
    [Test]
    public async Task OnRunCreated_ValidRun_ShouldAdd()
    {
        var messenger = new StrongReferenceMessenger();
        var statisticsMock = new Mock<IStatisticsRequestService>();
        var graphInfoMock = new Mock<IGraphInfoRequestService>();

        graphInfoMock
            .Setup(x => x.ReadGraphInfoAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GraphInformationModel());
        graphInfoMock
            .Setup(x => x.UpdateGraphInfoAsync(
                It.IsAny<GraphInformationModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        using var viewModel = CreateViewModel(messenger, statisticsMock, graphInfoMock);

        var completion = new TaskCompletionSource();
        messenger.Register<GraphStateChangedMessage>(this, (_, _) => completion.TrySetResult());

        var run = new RunStatisticsModel { Id = 1 };
        messenger.Send(new RunsCreatedMessaged([run]));

        await completion.Task.ConfigureAwait(false);

        Assert.That(viewModel.Runs, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task OnGraphActivated_ValidGraph_ShouldGetRuns()
    {
        var messenger = new StrongReferenceMessenger();
        var statisticsMock = new Mock<IStatisticsRequestService>();
        var graphInfoMock = new Mock<IGraphInfoRequestService>();

        IReadOnlyCollection<RunStatisticsModel> runs =
            [.. Enumerable.Range(1, 5).Select(x => new RunStatisticsModel { Id = x })];

        statisticsMock
            .Setup(x => x.ReadStatisticsAsync(
                It.Is<int>(id => id == 1),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(runs);

        using var viewModel = CreateViewModel(messenger, statisticsMock, graphInfoMock);

        await messenger.Send(CreateActivatedMessage(1), Tokens.RunsTable);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.Runs, Has.Count.EqualTo(runs.Count));
            Assert.That(viewModel.Runs, Is.Not.Empty);
        });
    }

    [Test]
    public async Task SelectRunCommand_ShouldSendMessage()
    {
        var messenger = new StrongReferenceMessenger();
        var statisticsMock = new Mock<IStatisticsRequestService>();
        var graphInfoMock = new Mock<IGraphInfoRequestService>();

        using var viewModel = CreateViewModel(messenger, statisticsMock, graphInfoMock);

        RunsSelectedMessage selectedMessage = null;
        messenger.Register<RunsSelectedMessage>(this, (_, msg) => selectedMessage = msg);

        await viewModel.SelectRunsCommand.Execute([]);

        Assert.That(selectedMessage, Is.Not.Null);
    }

    [Test]
    public async Task DeleteGraphsMessage_ActivatedGraph_ShouldClearRuns()
    {
        var messenger = new StrongReferenceMessenger();
        var statisticsMock = new Mock<IStatisticsRequestService>();
        var graphInfoMock = new Mock<IGraphInfoRequestService>();

        IReadOnlyCollection<RunStatisticsModel> runs =
            [.. Enumerable.Range(1, 5).Select(x => new RunStatisticsModel { Id = x })];

        statisticsMock
            .Setup(x => x.ReadStatisticsAsync(
                It.Is<int>(id => id == 1),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(runs);

        using var viewModel = CreateViewModel(messenger, statisticsMock, graphInfoMock);

        await messenger.Send(CreateActivatedMessage(1), Tokens.RunsTable);

        messenger.Send(new GraphsDeletedMessage([1]));

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.Runs, Is.Empty);
            statisticsMock
                .Verify(x => x.ReadStatisticsAsync(
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()), Times.Once);
        });
    }

    [Test]
    public async Task DeleteRunsMessage_SendAllRuns_ShouldDeleteAllAndSendEditableState()
    {
        var messenger = new StrongReferenceMessenger();
        var statisticsMock = new Mock<IStatisticsRequestService>();
        var graphInfoMock = new Mock<IGraphInfoRequestService>();

        IReadOnlyCollection<RunStatisticsModel> runs =
            [.. Enumerable.Range(1, 5).Select(x => new RunStatisticsModel { Id = x })];

        statisticsMock
            .Setup(x => x.ReadStatisticsAsync(
                It.Is<int>(id => id == 1),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(runs);

        graphInfoMock
            .Setup(x => x.ReadGraphInfoAsync(
                It.Is<int>(id => id == 1),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GraphInformationModel { Id = 1 });
        graphInfoMock
            .Setup(x => x.UpdateGraphInfoAsync(
                It.IsAny<GraphInformationModel>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        using var viewModel = CreateViewModel(messenger, statisticsMock, graphInfoMock);

        await messenger.Send(CreateActivatedMessage(1), Tokens.RunsTable);

        var stateChanged = new TaskCompletionSource();
        messenger.Register<GraphStateChangedMessage>(this, (_, msg) =>
        {
            if (msg.Value.Id == 1 && msg.Value.Status == GraphStatuses.Editable)
            {
                stateChanged.TrySetResult();
            }
        });

        messenger.Send(new RunsDeletedMessage([.. runs.Select(x => x.Id)]));

        await stateChanged.Task.ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.Runs, Is.Empty);
            graphInfoMock
                .Verify(x => x.UpdateGraphInfoAsync(
                    It.IsAny<GraphInformationModel>(),
                    It.IsAny<CancellationToken>()), Times.Once);
        });
    }

    [Test]
    public async Task ActivateGraph_ThrowsException_ShouldLogError()
    {
        var messenger = new StrongReferenceMessenger();
        var statisticsMock = new Mock<IStatisticsRequestService>();
        var graphInfoMock = new Mock<IGraphInfoRequestService>();
        var logMock = new Mock<ILog>();

        statisticsMock
            .Setup(x => x.ReadStatisticsAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        using var viewModel = CreateViewModel(messenger, statisticsMock, graphInfoMock, logMock.Object);

        await messenger.Send(CreateActivatedMessage(1), Tokens.RunsTable);

        logMock
            .Verify(x => x.Error(
                It.IsAny<Exception>(),
                It.IsAny<string>()), Times.Once);
    }

    private static RunsTableViewModel CreateViewModel(
        StrongReferenceMessenger messenger,
        Mock<IStatisticsRequestService> statisticsMock,
        Mock<IGraphInfoRequestService> graphInfoMock,
        ILog logger = null)
    {
        return new RunsTableViewModel(
            statisticsMock.Object,
            graphInfoMock.Object,
            messenger,
            logger ?? Mock.Of<ILog>());
    }

    private static AwaitGraphActivatedMessage CreateActivatedMessage(int graphId)
    {
        var activated = new ActivatedGraphModel(
            new(graphId, Graph<GraphVertexModel>.Empty, false),
            default,
            default);
        return new AwaitGraphActivatedMessage(activated);
    }
}
