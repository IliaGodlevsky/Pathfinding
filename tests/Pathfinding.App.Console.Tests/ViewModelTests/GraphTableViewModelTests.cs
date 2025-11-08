using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Factories;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Layers;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinding.App.Console.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class GraphTableViewModelTests
{
    [Test]
    public async Task LoadGraphsCommand_ShouldAddGraphs()
    {
        var messenger = new StrongReferenceMessenger();
        var graphServiceMock = new Mock<IGraphRequestService<GraphVertexModel>>();
        var graphInfoServiceMock = new Mock<IGraphInfoRequestService>();
        var neighborFactoryMock = new Mock<INeighborhoodLayerFactory>();

        IReadOnlyCollection<GraphInformationModel> graphs =
            [.. Enumerable.Range(1, 5).Select(x => new GraphInformationModel { Id = x, Dimensions = [] })];

        graphInfoServiceMock
            .Setup(x => x.ReadAllGraphInfoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(graphs);

        using var viewModel = CreateViewModel(
            messenger,
            graphServiceMock,
            graphInfoServiceMock,
            neighborFactoryMock);

        await viewModel.LoadGraphsCommand.Execute();

        Assert.Multiple(() =>
        {
            graphInfoServiceMock
                .Verify(x => x.ReadAllGraphInfoAsync(
                    It.IsAny<CancellationToken>()), Times.Once);
            Assert.That(viewModel.Graphs, Has.Count.EqualTo(graphs.Count));
        });
    }

    [Test]
    public async Task LoadGraphsCommand_ThrowsException_ShouldLogError()
    {
        var messenger = new StrongReferenceMessenger();
        var graphServiceMock = new Mock<IGraphRequestService<GraphVertexModel>>();
        var graphInfoServiceMock = new Mock<IGraphInfoRequestService>();
        var neighborFactoryMock = new Mock<INeighborhoodLayerFactory>();
        var logMock = new Mock<ILog>();

        graphInfoServiceMock
            .Setup(x => x.ReadAllGraphInfoAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        using var viewModel = CreateViewModel(
            messenger,
            graphServiceMock,
            graphInfoServiceMock,
            neighborFactoryMock,
            logMock.Object);

        await viewModel.LoadGraphsCommand.Execute();

        logMock
            .Verify(x => x.Error(
                It.IsAny<Exception>(),
                It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ActivateGraphCommand_ShouldSendMessage()
    {
        var messenger = new StrongReferenceMessenger();
        var graphServiceMock = new Mock<IGraphRequestService<GraphVertexModel>>();
        var graphInfoServiceMock = new Mock<IGraphInfoRequestService>();
        var neighborFactoryMock = new Mock<INeighborhoodLayerFactory>();

        var graph = new GraphModel<GraphVertexModel>
        {
            Id = 1,
            Name = "Test",
            Vertices = [],
            DimensionSizes = []
        };

        graphServiceMock
            .Setup(x => x.ReadGraphAsync(
                It.Is<int>(id => id == 1),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(graph);

        neighborFactoryMock
            .Setup(x => x.CreateNeighborhoodLayer(It.IsAny<Neighborhoods>()))
            .Returns(new MooreNeighborhoodLayer());

        var runsTableMessages = new List<AwaitGraphActivatedMessage>();
        messenger.Register<AwaitGraphActivatedMessage, int>(this, Tokens.RunsTable, (_, msg) =>
        {
            runsTableMessages.Add(msg);
            msg.SetCompleted();
        });
        var pathfindingMessages = new List<AwaitGraphActivatedMessage>();
        messenger.Register<AwaitGraphActivatedMessage, int>(this, Tokens.PathfindingRange, (_, msg) =>
        {
            pathfindingMessages.Add(msg);
            msg.SetCompleted();
        });
        var fieldMessages = new List<GraphActivatedMessage>();
        messenger.Register<GraphActivatedMessage, int>(this, Tokens.GraphField, (_, msg) => fieldMessages.Add(msg));
        var broadcastMessages = new List<GraphActivatedMessage>();
        messenger.Register<GraphActivatedMessage>(this, (_, msg) => broadcastMessages.Add(msg));

        using var viewModel = CreateViewModel(
            messenger,
            graphServiceMock,
            graphInfoServiceMock,
            neighborFactoryMock);

        await viewModel.ActivateGraphCommand.Execute(1);

        Assert.Multiple(() =>
        {
            graphServiceMock
                .Verify(x => x.ReadGraphAsync(
                    It.Is<int>(z => z == 1),
                    It.IsAny<CancellationToken>()), Times.Once);
            Assert.That(fieldMessages, Has.Count.EqualTo(1));
            Assert.That(runsTableMessages, Has.Count.EqualTo(1));
            Assert.That(pathfindingMessages, Has.Count.EqualTo(1));
            Assert.That(broadcastMessages, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task ActivateGraphCommand_ThrowException_ShouldLogError()
    {
        var messenger = new StrongReferenceMessenger();
        var graphServiceMock = new Mock<IGraphRequestService<GraphVertexModel>>();
        var graphInfoServiceMock = new Mock<IGraphInfoRequestService>();
        var neighborFactoryMock = new Mock<INeighborhoodLayerFactory>();
        var logMock = new Mock<ILog>();

        graphServiceMock
            .Setup(x => x.ReadGraphAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        using var viewModel = CreateViewModel(
            messenger,
            graphServiceMock,
            graphInfoServiceMock,
            neighborFactoryMock,
            logMock.Object);

        await viewModel.ActivateGraphCommand.Execute(1);

        logMock
            .Verify(x => x.Error(
                It.IsAny<Exception>(),
                It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task SelectedGraphsCommand_ShouldSendMessage()
    {
        var messenger = new StrongReferenceMessenger();
        var graphServiceMock = new Mock<IGraphRequestService<GraphVertexModel>>();
        var graphInfoServiceMock = new Mock<IGraphInfoRequestService>();
        var neighborFactoryMock = new Mock<INeighborhoodLayerFactory>();

        using var viewModel = CreateViewModel(
            messenger,
            graphServiceMock,
            graphInfoServiceMock,
            neighborFactoryMock);

        GraphsSelectedMessage? selectedMessage = null;
        messenger.Register<GraphsSelectedMessage>(this, (_, msg) => selectedMessage = msg);

        await viewModel.SelectGraphsCommand.Execute([1, 2, 3]);

        Assert.That(selectedMessage, Is.Not.Null);
    }

    private static GraphTableViewModel CreateViewModel(
        StrongReferenceMessenger messenger,
        Mock<IGraphRequestService<GraphVertexModel>> graphServiceMock,
        Mock<IGraphInfoRequestService> graphInfoServiceMock,
        Mock<INeighborhoodLayerFactory> neighborFactoryMock,
        ILog? logger = null)
    {
        neighborFactoryMock
            .SetupGet(x => x.Allowed)
            .Returns(Enum.GetValues<Neighborhoods>());
        neighborFactoryMock
            .Setup(x => x.CreateNeighborhoodLayer(It.IsAny<Neighborhoods>()))
            .Returns(new MooreNeighborhoodLayer());
        return new GraphTableViewModel(
            graphServiceMock.Object,
            graphInfoServiceMock.Object,
            neighborFactoryMock.Object,
            messenger,
            logger ?? Mock.Of<ILog>());
    }
}
