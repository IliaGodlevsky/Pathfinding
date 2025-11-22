using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using System.Reactive.Linq;

namespace Pathfinding.App.Console.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class DeleteGraphViewModelTests
{
    [Test]
    public async Task DeleteGraphCommand_MoreThanOneGraph_ShouldExecute()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IGraphInfoRequestService>();
        serviceMock
            .Setup(x => x.DeleteGraphsAsync(
                It.IsAny<IReadOnlyCollection<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        using var viewModel = CreateViewModel(messenger, serviceMock);

        var models = Generators.GenerateGraphInfos(3).ToArray();
        GraphsDeletedMessage deletedMessage = null;
        messenger.Register<GraphsDeletedMessage>(this, (_, msg) => deletedMessage = msg);

        messenger.Send(new GraphsSelectedMessage(models));

        var canExecute = await viewModel.DeleteGraphCommand.CanExecute.FirstAsync(value => value);

        await viewModel.DeleteGraphCommand.Execute();

        Assert.Multiple(() =>
        {
            Assert.That(canExecute, Is.True);
            serviceMock
                .Verify(x => x.DeleteGraphsAsync(
                    It.IsAny<IReadOnlyCollection<int>>(),
                    It.IsAny<CancellationToken>()), Times.Once);
            Assert.That(deletedMessage, Is.Not.Null);
            Assert.That(deletedMessage!.Value, Is.EqualTo(models.Select(x => x.Id).ToArray()));
        });
    }

    [Test]
    public async Task DeleteGraphCommand_ThrowsException_ShouldLogError()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IGraphInfoRequestService>();
        serviceMock
            .Setup(x => x.DeleteGraphsAsync(
                It.IsAny<IReadOnlyCollection<int>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        var logMock = new Mock<ILog>();

        using var viewModel = CreateViewModel(messenger, serviceMock, logMock.Object);

        messenger.Send(new GraphsSelectedMessage([.. Generators.GenerateGraphInfos(1)]));

        await viewModel.DeleteGraphCommand.Execute();

        logMock
            .Verify(x => x.Error(
                It.IsAny<Exception>(),
                It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task DeleteGraphCommand_NoGraphs_ShouldNotExecute()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IGraphInfoRequestService>();

        using var viewModel = CreateViewModel(messenger, serviceMock);

        var canExecute = await viewModel.DeleteGraphCommand.CanExecute.FirstAsync(value => !value);

        Assert.Multiple(() =>
        {
            Assert.That(canExecute, Is.False);
            serviceMock
                .Verify(x => x.DeleteGraphsAsync(
                    It.IsAny<IReadOnlyCollection<int>>(),
                    It.IsAny<CancellationToken>()), Times.Never);
        });
    }

    private static GraphDeleteViewModel CreateViewModel(StrongReferenceMessenger messenger,
        Mock<IGraphInfoRequestService> serviceMock, ILog logger = null)
    {
        return new GraphDeleteViewModel(messenger, serviceMock.Object, logger ?? Mock.Of<ILog>());
    }
}
