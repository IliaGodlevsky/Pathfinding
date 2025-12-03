using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Messages.ViewModel.Requests;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Requests.Update;
using Pathfinding.Shared.Primitives;
using System.Reactive.Linq;

namespace Pathfinding.App.Console.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class GraphFieldViewModelTests
{
    [Test]
    public async Task AwaitGraphActivatedMessage_EditableGraph_ShouldEnableCommands()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IRequestService<GraphVertexModel>>();
        using var viewModel = CreateViewModel(messenger, serviceMock);

        var vertex = CreateVertex();
        var graph = new Graph<GraphVertexModel>([vertex], 1);
        var message = new AwaitGraphActivatedMessage(new ActivatedGraphModel(
            new(1, graph, false),
            default,
            default));

        await messenger.Send(message);

        var canExecute = await viewModel.ChangeVertexPolarityCommand.CanExecute.FirstAsync(value => value);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.ActivatedGraph.Graph, Is.EqualTo(graph));
            Assert.That(canExecute, Is.True);
        });
    }

    [Test]
    public async Task AwaitGraphActivatedMessage_ReadonlyGraph_ShouldDisableCommands()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IRequestService<GraphVertexModel>>();
        using var viewModel = CreateViewModel(messenger, serviceMock);

        var vertex = CreateVertex();
        var graph = new Graph<GraphVertexModel>([vertex], 1);
        var message = new AwaitGraphActivatedMessage(new ActivatedGraphModel(
            new(1, graph, true),
            default,
            default));

        await messenger.Send(message);

        var canExecute = await viewModel.ChangeVertexPolarityCommand.CanExecute.FirstAsync(value => !value);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.ActivatedGraph.Graph, Is.EqualTo(graph));
            Assert.That(canExecute, Is.False);
        });
    }

    [Test]
    public async Task ChangeVertexPolarityCommand_VertexNotInRange_ShouldUpdateVertex()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IRequestService<GraphVertexModel>>();
        serviceMock
            .Setup(x => x.UpdateVerticesAsync(
                It.IsAny<UpdateVerticesRequest<GraphVertexModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        using var viewModel = CreateViewModel(messenger, serviceMock);

        var vertex = CreateVertex();
        var graph = new Graph<GraphVertexModel>([vertex], 1);
        var message = new AwaitGraphActivatedMessage(new ActivatedGraphModel(
            new(2, graph, false),
            default,
            default));

        ObstaclesCountChangedMessage obstaclesMessage = null;
        messenger.Register<ObstaclesCountChangedMessage>(this, (_, msg) => obstaclesMessage = msg);
        messenger.Register<IsVertexInRangeRequestMessage>(this, (_, request) => request.Reply(false));

        await messenger.Send(message);

        await viewModel.ChangeVertexPolarityCommand.Execute(vertex);

        Assert.Multiple(() =>
        {
            Assert.That(vertex.IsObstacle, Is.True);
            serviceMock
                .Verify(x => x.UpdateVerticesAsync(
                    It.Is<UpdateVerticesRequest<GraphVertexModel>>(request
                        => request.GraphId == 2
                        && request.Vertices.Single() == vertex),
                    It.IsAny<CancellationToken>()), Times.Once);
            Assert.That(obstaclesMessage, Is.Not.Null);
            Assert.That(obstaclesMessage!.Value.GraphId, Is.EqualTo(2));
            Assert.That(obstaclesMessage!.Value.Delta, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task ChangeVertexPolarityCommand_VertexInRange_ShouldNotUpdateVertex()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IRequestService<GraphVertexModel>>();
        using var viewModel = CreateViewModel(messenger, serviceMock);

        var vertex = CreateVertex();
        var graph = new Graph<GraphVertexModel>([vertex], 1);
        var message = new AwaitGraphActivatedMessage(new ActivatedGraphModel(
            new(3, graph, false),
            default,
            default));

        var obstaclesSent = false;
        messenger.Register<ObstaclesCountChangedMessage>(this, (_, _) => obstaclesSent = true);
        messenger.Register<IsVertexInRangeRequestMessage>(this, (_, request) => request.Reply(true));

        await messenger.Send(message);

        await viewModel.ChangeVertexPolarityCommand.Execute(vertex);

        Assert.Multiple(() =>
        {
            Assert.That(vertex.IsObstacle, Is.False);
            serviceMock
                .Verify(x => x.UpdateVerticesAsync(
                    It.IsAny<UpdateVerticesRequest<GraphVertexModel>>(),
                    It.IsAny<CancellationToken>()), Times.Never);
            Assert.That(obstaclesSent, Is.False);
        });
    }

    [Test]
    public async Task ChangeVertexCostCommand_ShouldClampAndPersistCost()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IRequestService<GraphVertexModel>>();
        serviceMock
            .Setup(x => x.UpdateVerticesAsync(
                It.IsAny<UpdateVerticesRequest<GraphVertexModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        using var viewModel = CreateViewModel(messenger, serviceMock);

        var vertex = CreateVertex();
        vertex.Cost = new VertexCost(9, new InclusiveValueRange<int>(10, 0));
        var graph = new Graph<GraphVertexModel>([vertex], 1);
        var message = new AwaitGraphActivatedMessage(new ActivatedGraphModel(
            new(4, graph, false),
            default,
            default));

        await messenger.Send(message);

        await viewModel.IncreaseVertexCostCommand.Execute(vertex);

        Assert.Multiple(() =>
        {
            Assert.That(vertex.Cost.CurrentCost, Is.EqualTo(10));
            serviceMock
                .Verify(x => x.UpdateVerticesAsync(
                    It.Is<UpdateVerticesRequest<GraphVertexModel>>(request
                        => request.GraphId == 4
                        && request.Vertices.Single() == vertex),
                    It.IsAny<CancellationToken>()), Times.Once);
        });
    }

    [Test]
    public async Task InverseVertexCommand_ShouldDecreaseObstaclesCount()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IRequestService<GraphVertexModel>>();
        serviceMock
            .Setup(x => x.UpdateVerticesAsync(
                It.IsAny<UpdateVerticesRequest<GraphVertexModel>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        using var viewModel = CreateViewModel(messenger, serviceMock);

        var vertex = CreateVertex();
        vertex.IsObstacle = true;
        var graph = new Graph<GraphVertexModel>([vertex], 1);
        var message = new AwaitGraphActivatedMessage(new ActivatedGraphModel(
            new(5, graph, false),
            default,
            default));

        ObstaclesCountChangedMessage obstaclesMessage = null;
        messenger.Register<ObstaclesCountChangedMessage>(this, (_, msg) => obstaclesMessage = msg);
        messenger.Register<IsVertexInRangeRequestMessage>(this, (_, request) => request.Reply(false));

        await messenger.Send(message);

        await viewModel.InverseVertexCommand.Execute(vertex);

        Assert.Multiple(() =>
        {
            Assert.That(vertex.IsObstacle, Is.False);
            Assert.That(obstaclesMessage, Is.Not.Null);
            Assert.That(obstaclesMessage!.Value.GraphId, Is.EqualTo(5));
            Assert.That(obstaclesMessage!.Value.Delta, Is.EqualTo(-1));
        });
    }

    [Test]
    public async Task GraphStateChangedMessage_ShouldToggleCommandAvailability()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IRequestService<GraphVertexModel>>();
        using var viewModel = CreateViewModel(messenger, serviceMock);

        var vertex = CreateVertex();
        var graph = new Graph<GraphVertexModel>([vertex], 1);
        var message = new AwaitGraphActivatedMessage(new ActivatedGraphModel(
            new(6, graph, false),
            default,
            default));

        await messenger.Send(message);

        var enabled = await viewModel.ChangeVertexPolarityCommand.CanExecute.FirstAsync(value => value);

        messenger.Send(new GraphStateChangedMessage((6, GraphStatuses.Readonly)));

        var disabled = await viewModel.ChangeVertexPolarityCommand.CanExecute.FirstAsync(value => !value);

        Assert.Multiple(() =>
        {
            Assert.That(enabled, Is.True);
            Assert.That(disabled, Is.False);
        });
    }

    [Test]
    public async Task GraphsDeletedMessage_ShouldResetGraphAndDisableCommands()
    {
        var messenger = new StrongReferenceMessenger();
        var serviceMock = new Mock<IRequestService<GraphVertexModel>>();
        using var viewModel = CreateViewModel(messenger, serviceMock);

        var vertex = CreateVertex();
        var graph = new Graph<GraphVertexModel>([vertex], 1);
        var message = new AwaitGraphActivatedMessage(new ActivatedGraphModel(
            new(7, graph, false),
            default,
            default));

        await messenger.Send(message);

        var enabled = await viewModel.ChangeVertexPolarityCommand.CanExecute.FirstAsync(value => value);
        Assert.That(enabled, Is.True);

        messenger.Send(new GraphsDeletedMessage([7]));

        var disabled = await viewModel.ChangeVertexPolarityCommand.CanExecute.FirstAsync(value => !value);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.ActivatedGraph.Graph, Is.SameAs(Graph<GraphVertexModel>.Empty));
            Assert.That(disabled, Is.False);
        });
    }

    private static GraphFieldViewModel CreateViewModel(StrongReferenceMessenger messenger,
        Mock<IRequestService<GraphVertexModel>> serviceMock)
    {
        return new GraphFieldViewModel(messenger, serviceMock.Object, Mock.Of<ILog>());
    }

    private static GraphVertexModel CreateVertex()
    {
        return new GraphVertexModel
        {
            Id = 1,
            IsObstacle = false,
            Position = new Coordinate(0),
            Cost = new VertexCost(1, new InclusiveValueRange<int>(10, 0))
        };
    }
}
