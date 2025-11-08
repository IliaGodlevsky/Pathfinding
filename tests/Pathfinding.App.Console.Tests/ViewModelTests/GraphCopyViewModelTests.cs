using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Models.Serialization;
using Pathfinding.Shared.Extensions;
using System.Reactive.Linq;

namespace Pathfinding.App.Console.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class GraphCopyViewModelTests
{
    [Test]
    public async Task CopyCommand_CanExecute_ShouldCopy()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IGraphRequestService<GraphVertexModel>>();

        var models = Generators.GenerateGraphInfos(3).ToArray();

        var histories = Enumerable.Range(1, 5)
            .Select(_ => new PathfindingHistorySerializationModel())
            .ToArray()
            .To(x => new PathfindingHistoriesSerializationModel { Histories = [.. x] });
        var createdHistories = Enumerable.Range(1, 5)
            .Select(index => new PathfindingHistoryModel<GraphVertexModel>
            {
                Graph = new GraphModel<GraphVertexModel>
                {
                    Id = index,
                    Vertices = [],
                    DimensionSizes = [],
                    Name = string.Empty
                }
            })
            .ToArray()
            .To<IReadOnlyCollection<PathfindingHistoryModel<GraphVertexModel>>>();

        serviceMock
            .Setup(x => x.ReadSerializationHistoriesAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(histories);

        serviceMock
            .Setup(x => x.CreatePathfindingHistoriesAsync(
                It.IsAny<IEnumerable<PathfindingHistorySerializationModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdHistories);

        using var viewModel = CreateViewModel(messenger, serviceMock);

        GraphsCreatedMessage? createdMessage = null;
        messenger.Register<GraphsCreatedMessage>(this, (_, msg) => createdMessage = msg);

        messenger.Send(new GraphsSelectedMessage(models));

        if (await viewModel.CopyGraphCommand.CanExecute.FirstAsync(value => value))
        {
            await viewModel.CopyGraphCommand.Execute();
        }

        Assert.Multiple(() =>
        {
            serviceMock
                .Verify(x => x.ReadSerializationHistoriesAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<CancellationToken>()), Times.Once);

            serviceMock
                .Verify(x => x.CreatePathfindingHistoriesAsync(
                    It.IsAny<IEnumerable<PathfindingHistorySerializationModel>>(),
                    It.IsAny<CancellationToken>()), Times.Once);

            Assert.That(createdMessage, Is.Not.Null);
        });
    }

    [Test]
    public async Task CopyGraphCommand_ThrowsException_ShouldLogError()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IGraphRequestService<GraphVertexModel>>();

        serviceMock
            .Setup(x => x.ReadSerializationHistoriesAsync(
                It.IsAny<IEnumerable<int>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        var logMock = new Mock<ILog>();

        using var viewModel = CreateViewModel(messenger, serviceMock, logMock.Object);

        messenger.Send(new GraphsSelectedMessage(Generators.GenerateGraphInfos(1).ToArray()));

        await viewModel.CopyGraphCommand.Execute();

        logMock
            .Verify(x => x.Error(
                It.IsAny<Exception>(),
                It.IsAny<string>()), Times.Once);
    }

    private static GraphCopyViewModel CreateViewModel(StrongReferenceMessenger messenger,
        Mock<IGraphRequestService<GraphVertexModel>> serviceMock, ILog? logger = null)
    {
        return new GraphCopyViewModel(messenger, serviceMock.Object, logger ?? Mock.Of<ILog>());
    }
}
