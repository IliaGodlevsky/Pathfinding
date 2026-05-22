using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.Data;
using Pathfinding.Domain.Enums;
using Pathfinding.Logging.Interface;
using Pathfinding.Presentation.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.Presentation.Console.Models;
using Pathfinding.Presentation.Console.ViewModels;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Models.Undefined;
using Pathfinding.Service.Interface.Requests.Read;
using System.Reactive.Linq;

namespace Pathfinding.Presentation.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class RunTableViewModelTests
{
    [Test]
    public async Task OnRunCreated_ValidRun_ShouldAdd()
    {
        var messenger = new StrongReferenceMessenger();
        var statisticsMock = new Mock<IStatisticsRequestService>();

        using var viewModel = CreateViewModel(messenger, statisticsMock);

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

        IReadOnlyCollection<RunStatisticsModel> runs =
            [.. Enumerable.Range(1, 5).Select(x => new RunStatisticsModel { Id = x })];

        statisticsMock
            .Setup(x => x.ReadStatisticsAsync(
                It.Is<ReadStatisticsRequest>(r => r.GraphId == 1),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(runs);

        using var viewModel = CreateViewModel(messenger, statisticsMock);

        await messenger.Send(CreateActivatedMessage(1));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewModel.Runs, Has.Count.EqualTo(runs.Count));
            Assert.That(viewModel.Runs, Is.Not.Empty);
        }
    }

    [Test]
    public async Task SelectRunCommand_ShouldSendMessage()
    {
        var messenger = new StrongReferenceMessenger();
        var statisticsMock = new Mock<IStatisticsRequestService>();

        using var viewModel = CreateViewModel(messenger, statisticsMock);

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

        IReadOnlyCollection<RunStatisticsModel> runs =
            [.. Enumerable.Range(1, 5).Select(x => new RunStatisticsModel { Id = x })];

        statisticsMock
            .Setup(x => x.ReadStatisticsAsync(
                It.Is<ReadStatisticsRequest>(r => r.GraphId == 1),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(runs);

        using var viewModel = CreateViewModel(messenger, statisticsMock);

        await messenger.Send(CreateActivatedMessage(1));

        messenger.Send(new GraphsDeletedMessage([1]));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewModel.Runs, Is.Empty);
            statisticsMock
                .Verify(x => x.ReadStatisticsAsync(
                    It.IsAny<ReadStatisticsRequest>(),
                    It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Test]
    public async Task DeleteRunsMessage_SendAllRuns_ShouldDeleteAllAndSendEditableState()
    {
        var messenger = new StrongReferenceMessenger();
        var statisticsMock = new Mock<IStatisticsRequestService>();

        IReadOnlyCollection<RunStatisticsModel> runs =
            [.. Enumerable.Range(1, 5).Select(x => new RunStatisticsModel { Id = x })];

        statisticsMock
            .Setup(x => x.ReadStatisticsAsync(
                It.Is<ReadStatisticsRequest>(r => r.GraphId == 1),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(runs);

        using var viewModel = CreateViewModel(messenger, statisticsMock);

        await messenger.Send(CreateActivatedMessage(1));

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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewModel.Runs, Is.Empty);
        }
    }

    [Test]
    public async Task ActivateGraph_ThrowsException_ShouldLogError()
    {
        var messenger = new StrongReferenceMessenger();
        var statisticsMock = new Mock<IStatisticsRequestService>();
        var logMock = new Mock<ILog>();

        statisticsMock
            .Setup(x => x.ReadStatisticsAsync(
                It.IsAny<ReadStatisticsRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        using var viewModel = CreateViewModel(messenger, statisticsMock, logMock.Object);

        await messenger.Send(CreateActivatedMessage(1));

        logMock
            .Verify(x => x.Error(
                It.IsAny<Exception>(),
                It.IsAny<string>()), Times.Once);
    }

    private static RunsTableViewModel CreateViewModel(
        StrongReferenceMessenger messenger,
        Mock<IStatisticsRequestService> statisticsMock,
        ILog logger = null)
    {
        return new RunsTableViewModel(
            statisticsMock.Object,
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
