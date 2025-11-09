using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Factories;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using System.Reactive.Linq;

namespace Pathfinding.App.Console.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class GraphUpdateViewModelTests
{
    [Test]
    public void GraphsSelectedMessage_ShouldPopulateSelection()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IGraphInfoRequestService>();
        var neighborhoodFactoryMock = new Mock<INeighborhoodLayerFactory>();
        neighborhoodFactoryMock.SetupGet(x => x.Allowed)
            .Returns([Neighborhoods.Moore, Neighborhoods.Diagonal]);

        using var viewModel = CreateViewModel(messenger, serviceMock, neighborhoodFactoryMock);

        var model = Generators.GenerateGraphInfos(1).Single();
        messenger.Send(new GraphsSelectedMessage([model]));

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.SelectedGraphs, Has.Length.EqualTo(1));
            Assert.That(viewModel.Name, Is.EqualTo(model.Name));
            Assert.That(viewModel.Neighborhood, Is.EqualTo(model.Neighborhood));
            Assert.That(viewModel.Status, Is.EqualTo(model.Status));
        });
    }

    [Test]
    public void GraphsDeletedMessage_ShouldClearNameWhenSelectionRemoved()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IGraphInfoRequestService>();
        var neighborhoodFactoryMock = new Mock<INeighborhoodLayerFactory>();
        neighborhoodFactoryMock.SetupGet(x => x.Allowed).Returns([]);

        using var viewModel = CreateViewModel(messenger, serviceMock, neighborhoodFactoryMock);

        var model = Generators.GenerateGraphInfos(1).Single();
        viewModel.SelectedGraphs = [model];
        viewModel.Name = "Graph";

        messenger.Send(new GraphsDeletedMessage([model.Id]));

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.SelectedGraphs, Is.Empty);
            Assert.That(viewModel.Name, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public async Task UpdateGraphCommand_ShouldUpdateGraphAndNotifyAsync()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IGraphInfoRequestService>();
        var neighborhoodFactoryMock = new Mock<INeighborhoodLayerFactory>();
        neighborhoodFactoryMock.SetupGet(x => x.Allowed)
            .Returns([Neighborhoods.Moore]);

        var graphInfoModel = Generators.GenerateGraphInfos(1).Single();
        var newName = graphInfoModel.Name + "_updated";
        var graphInformation = new GraphInformationModel
        {
            Id = graphInfoModel.Id,
            Name = graphInfoModel.Name,
            Neighborhood = graphInfoModel.Neighborhood,
            Status = graphInfoModel.Status,
            Dimensions = []
        };

        serviceMock
            .Setup(x => x.ReadGraphInfoAsync(
                graphInfoModel.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(graphInformation);

        serviceMock
            .Setup(x => x.UpdateGraphInfoAsync(
                It.IsAny<GraphInformationModel>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true));

        var graphTableMessages = new List<AwaitGraphUpdatedMessage>();
        messenger.Register<AwaitGraphUpdatedMessage, int>(this, Tokens.GraphTable, (_, msg) =>
        {
            graphTableMessages.Add(msg);
            msg.SetCompleted();
        });

        var algorithmUpdateMessages = new List<AwaitGraphUpdatedMessage>();
        messenger.Register<AwaitGraphUpdatedMessage, int>(this, Tokens.AlgorithmUpdate, (_, msg) =>
        {
            algorithmUpdateMessages.Add(msg);
            msg.SetCompleted();
        });

        GraphUpdatedMessage broadcastMessage = null;
        messenger.Register<GraphUpdatedMessage>(this, (_, msg) => broadcastMessage = msg);

        using var viewModel = CreateViewModel(messenger, serviceMock, neighborhoodFactoryMock);

        messenger.Send(new GraphsSelectedMessage([graphInfoModel]));
        viewModel.Name = newName;
        viewModel.Neighborhood = Neighborhoods.Moore;

        await viewModel.UpdateGraphCommand.Execute();

        Assert.Multiple(() =>
        {
            serviceMock.Verify(x => x.ReadGraphInfoAsync(
                graphInfoModel.Id,
                It.IsAny<CancellationToken>()), Times.Once);
            serviceMock.Verify(x => x.UpdateGraphInfoAsync(
                It.Is<GraphInformationModel>(model
                    => model.Id == graphInfoModel.Id
                       && model.Name == newName
                       && model.Neighborhood == Neighborhoods.Moore),
                It.IsAny<CancellationToken>()), Times.Once);
            Assert.That(graphTableMessages, Has.Count.EqualTo(1));
            Assert.That(algorithmUpdateMessages, Has.Count.EqualTo(1));
            Assert.That(broadcastMessage, Is.Not.Null);
        });
    }

    private static GraphUpdateViewModel CreateViewModel(
        IMessenger messenger,
        Mock<IGraphInfoRequestService> serviceMock,
        Mock<INeighborhoodLayerFactory> neighborhoodFactoryMock,
        ILog log = null)
    {
        return new GraphUpdateViewModel(
            serviceMock.Object,
            neighborhoodFactoryMock.Object,
            messenger,
            log ?? Mock.Of<ILog>());
    }
}
