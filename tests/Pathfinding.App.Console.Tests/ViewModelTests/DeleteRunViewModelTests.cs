using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using System.Reactive.Linq;

namespace Pathfinding.App.Console.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class DeleteRunViewModelTests
{
    [Test]
    public async Task DeleteRunsCommand_SeveralRunIds_ShouldDelete()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IStatisticsRequestService>();
        serviceMock
            .Setup(x => x.DeleteRunsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        using var viewModel = CreateViewModel(messenger, serviceMock);

        var runModels = new RunInfoModel[]
        {
            new() { Id = 1 },
            new() { Id = 2 },
            new() { Id = 3 }
        };

        RunsDeletedMessage deletedMessage = null;
        messenger.Register<RunsDeletedMessage>(this, (_, msg) => deletedMessage = msg);

        messenger.Send(new RunsSelectedMessage(runModels));

        var canExecute = await viewModel.DeleteRunsCommand.CanExecute.FirstAsync(value => value);

        await viewModel.DeleteRunsCommand.Execute();

        Assert.Multiple(() =>
        {
            Assert.That(canExecute, Is.True);
            serviceMock
                .Verify(x => x.DeleteRunsAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<CancellationToken>()), Times.Once);
            Assert.That(deletedMessage, Is.Not.Null);
            Assert.That(deletedMessage!.Value, Is.EqualTo(runModels.Select(x => x.Id).ToArray()));
        });
    }

    [Test]
    public async Task DeleteRunsCommand_ThrowsException_ShouldLogError()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IStatisticsRequestService>();
        serviceMock
            .Setup(x => x.DeleteRunsAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        var logMock = new Mock<ILog>();

        using var viewModel = CreateViewModel(messenger, serviceMock, logMock.Object);

        messenger.Send(new RunsSelectedMessage([new RunInfoModel { Id = 1 }]));

        await viewModel.DeleteRunsCommand.Execute();

        logMock
            .Verify(x => x.Error(
                It.IsAny<Exception>(),
                It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task DeleteRunsCommand_NoRuns_ShouldNotExecute()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IStatisticsRequestService>();

        using var viewModel = CreateViewModel(messenger, serviceMock);

        var canExecute = await viewModel.DeleteRunsCommand.CanExecute.FirstAsync(value => !value);

        Assert.Multiple(() =>
        {
            Assert.That(canExecute, Is.False);
            serviceMock
                .Verify(x => x.DeleteRunsAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    private static RunDeleteViewModel CreateViewModel(StrongReferenceMessenger messenger,
        Mock<IStatisticsRequestService> serviceMock, ILog logger = null)
    {
        return new RunDeleteViewModel(messenger, serviceMock.Object, logger ?? Mock.Of<ILog>());
    }
}
