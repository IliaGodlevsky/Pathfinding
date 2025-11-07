using Autofac.Extras.Moq;
using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Messages.ViewModel.Requests;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Requests.Update;
using Pathfinding.Shared.Primitives;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pathfinding.App.Console.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class GraphFieldViewModelTests
{
    [Test]
    public async Task GraphActivatedMessage_EditableGraph_ShouldEnableCommands()
    {
        using var mock = AutoMock.GetLoose();

        var vertex = CreateVertex();
        var graph = new Graph<GraphVertexModel>([vertex], 1);
        var message = new GraphActivatedMessage(new ActivatedGraphModel(
            graph,
            default,
            default,
            GraphStatuses.Editable,
            1));

        mock.Mock<IMessenger>()
            .Setup(x => x.Register(
                It.IsAny<object>(),
                It.Is<int>(token => token == Tokens.GraphField),
                It.IsAny<MessageHandler<object, GraphActivatedMessage>>()))
            .Callback<object, int, MessageHandler<object, GraphActivatedMessage>>((recipient, _, handler)
                => handler(recipient, message));

        var viewModel = mock.Create<GraphFieldViewModel>();

        var canExecute = await viewModel.ChangeVertexPolarityCommand.CanExecute.FirstAsync();

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.Graph, Is.EqualTo(graph));
            Assert.That(canExecute, Is.True);
        });
    }

    [Test]
    public async Task GraphActivatedMessage_ReadonlyGraph_ShouldDisableCommands()
    {
        using var mock = AutoMock.GetLoose();

        var vertex = CreateVertex();
        var graph = new Graph<GraphVertexModel>([vertex], 1);
        var message = new GraphActivatedMessage(new ActivatedGraphModel(
            graph,
            default,
            default,
            GraphStatuses.Readonly,
            1));

        mock.Mock<IMessenger>()
            .Setup(x => x.Register(
                It.IsAny<object>(),
                It.Is<int>(token => token == Tokens.GraphField),
                It.IsAny<MessageHandler<object, GraphActivatedMessage>>()))
            .Callback<object, int, MessageHandler<object, GraphActivatedMessage>>((recipient, _, handler)
                => handler(recipient, message));

        var viewModel = mock.Create<GraphFieldViewModel>();

        var canExecute = await viewModel.ChangeVertexPolarityCommand.CanExecute.FirstAsync();

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.Graph, Is.EqualTo(graph));
            Assert.That(canExecute, Is.False);
        });
    }

    [Test]
    public async Task ChangeVertexPolarityCommand_VertexNotInRange_ShouldUpdateVertex()
    {
        using var mock = AutoMock.GetLoose();

        var vertex = CreateVertex();
        var graph = new Graph<GraphVertexModel>([vertex], 1);
        var message = new GraphActivatedMessage(new ActivatedGraphModel(
            graph,
            default,
            default,
            GraphStatuses.Editable,
            2));

        mock.Mock<IMessenger>()
            .Setup(x => x.Register(
                It.IsAny<object>(),
                It.Is<int>(token => token == Tokens.GraphField),
                It.IsAny<MessageHandler<object, GraphActivatedMessage>>()))
            .Callback<object, int, MessageHandler<object, GraphActivatedMessage>>((recipient, _, handler)
                => handler(recipient, message));

        mock.Mock<IMessenger>()
            .Setup(x => x.Send(It.IsAny<IsVertexInRangeRequestMessage>()))
            .Callback<IsVertexInRangeRequestMessage>(request => request.Reply(false))
            .Returns((IsVertexInRangeRequestMessage request) => request);

        mock.Mock<IRequestService<GraphVertexModel>>()
            .Setup(x => x.UpdateVerticesAsync(
                It.IsAny<UpdateVerticesRequest<GraphVertexModel>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var viewModel = mock.Create<GraphFieldViewModel>();

        await viewModel.ChangeVertexPolarityCommand.Execute(vertex);

        Assert.Multiple(() =>
        {
            Assert.That(vertex.IsObstacle, Is.True);
            mock.Mock<IRequestService<GraphVertexModel>>()
                .Verify(x => x.UpdateVerticesAsync(
                    It.Is<UpdateVerticesRequest<GraphVertexModel>>(request
                        => request.GraphId == 2
                        && request.Vertices.Single() == vertex),
                    It.IsAny<CancellationToken>()), Times.Once);
            mock.Mock<IMessenger>()
                .Verify(x => x.Send(
                    It.Is<ObstaclesCountChangedMessage>(msg
                        => msg.Value.GraphId == 2
                        && msg.Value.Delta == 1)), Times.Once);
        });
    }

    [Test]
    public async Task ChangeVertexPolarityCommand_VertexInRange_ShouldNotUpdateVertex()
    {
        using var mock = AutoMock.GetLoose();

        var vertex = CreateVertex();
        var graph = new Graph<GraphVertexModel>([vertex], 1);
        var message = new GraphActivatedMessage(new ActivatedGraphModel(
            graph,
            default,
            default,
            GraphStatuses.Editable,
            3));

        mock.Mock<IMessenger>()
            .Setup(x => x.Register(
                It.IsAny<object>(),
                It.Is<int>(token => token == Tokens.GraphField),
                It.IsAny<MessageHandler<object, GraphActivatedMessage>>()))
            .Callback<object, int, MessageHandler<object, GraphActivatedMessage>>((recipient, _, handler)
                => handler(recipient, message));

        mock.Mock<IMessenger>()
            .Setup(x => x.Send(It.IsAny<IsVertexInRangeRequestMessage>()))
            .Callback<IsVertexInRangeRequestMessage>(request => request.Reply(true))
            .Returns((IsVertexInRangeRequestMessage request) => request);

        var viewModel = mock.Create<GraphFieldViewModel>();

        await viewModel.ChangeVertexPolarityCommand.Execute(vertex);

        Assert.Multiple(() =>
        {
            Assert.That(vertex.IsObstacle, Is.False);
            mock.Mock<IRequestService<GraphVertexModel>>()
                .Verify(x => x.UpdateVerticesAsync(
                    It.IsAny<UpdateVerticesRequest<GraphVertexModel>>(),
                    It.IsAny<CancellationToken>()), Times.Never);
            mock.Mock<IMessenger>()
                .Verify(x => x.Send(It.IsAny<ObstaclesCountChangedMessage>()), Times.Never);
        });
    }

    [Test]
    public async Task ChangeVertexCostCommand_ShouldClampAndPersistCost()
    {
        using var mock = AutoMock.GetLoose();

        var vertex = CreateVertex();
        vertex.Cost = new VertexCost(9, new InclusiveValueRange<int>(10, 0));
        var graph = new Graph<GraphVertexModel>([vertex], 1);
        var message = new GraphActivatedMessage(new ActivatedGraphModel(
            graph,
            default,
            default,
            GraphStatuses.Editable,
            4));

        mock.Mock<IMessenger>()
            .Setup(x => x.Register(
                It.IsAny<object>(),
                It.Is<int>(token => token == Tokens.GraphField),
                It.IsAny<MessageHandler<object, GraphActivatedMessage>>()))
            .Callback<object, int, MessageHandler<object, GraphActivatedMessage>>((recipient, _, handler)
                => handler(recipient, message));

        mock.Mock<IRequestService<GraphVertexModel>>()
            .Setup(x => x.UpdateVerticesAsync(
                It.IsAny<UpdateVerticesRequest<GraphVertexModel>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var viewModel = mock.Create<GraphFieldViewModel>();

        await viewModel.IncreaseVertexCostCommand.Execute(vertex);

        Assert.Multiple(() =>
        {
            Assert.That(vertex.Cost.CurrentCost, Is.EqualTo(10));
            mock.Mock<IRequestService<GraphVertexModel>>()
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
        using var mock = AutoMock.GetLoose();

        var vertex = CreateVertex();
        vertex.IsObstacle = true;
        var graph = new Graph<GraphVertexModel>([vertex], 1);
        var message = new GraphActivatedMessage(new ActivatedGraphModel(
            graph,
            default,
            default,
            GraphStatuses.Editable,
            5));

        mock.Mock<IMessenger>()
            .Setup(x => x.Register(
                It.IsAny<object>(),
                It.Is<int>(token => token == Tokens.GraphField),
                It.IsAny<MessageHandler<object, GraphActivatedMessage>>()))
            .Callback<object, int, MessageHandler<object, GraphActivatedMessage>>((recipient, _, handler)
                => handler(recipient, message));

        mock.Mock<IMessenger>()
            .Setup(x => x.Send(It.IsAny<IsVertexInRangeRequestMessage>()))
            .Callback<IsVertexInRangeRequestMessage>(request => request.Reply(false))
            .Returns((IsVertexInRangeRequestMessage request) => request);

        mock.Mock<IRequestService<GraphVertexModel>>()
            .Setup(x => x.UpdateVerticesAsync(
                It.IsAny<UpdateVerticesRequest<GraphVertexModel>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var viewModel = mock.Create<GraphFieldViewModel>();

        await viewModel.InverseVertexCommand.Execute(vertex);

        Assert.Multiple(() =>
        {
            Assert.That(vertex.IsObstacle, Is.False);
            mock.Mock<IMessenger>()
                .Verify(x => x.Send(
                    It.Is<ObstaclesCountChangedMessage>(msg
                        => msg.Value.GraphId == 5
                        && msg.Value.Delta == -1)), Times.Once);
        });
    }

    [Test]
    public async Task GraphStateChangedMessage_ShouldToggleCommandAvailability()
    {
        using var mock = AutoMock.GetLoose();

        var vertex = CreateVertex();
        var graph = new Graph<GraphVertexModel>([vertex], 1);
        var activatedMessage = new GraphActivatedMessage(new ActivatedGraphModel(
            graph,
            default,
            default,
            GraphStatuses.Editable,
            6));

        MessageHandler<object, GraphStateChangedMessage>? handler = null;

        mock.Mock<IMessenger>()
            .Setup(x => x.Register(
                It.IsAny<object>(),
                It.Is<int>(token => token == Tokens.GraphField),
                It.IsAny<MessageHandler<object, GraphActivatedMessage>>()))
            .Callback<object, int, MessageHandler<object, GraphActivatedMessage>>((recipient, _, action)
                => action(recipient, activatedMessage));

        mock.Mock<IMessenger>()
            .Setup(x => x.Register(
                It.IsAny<object>(),
                It.IsAny<MessageHandler<object, GraphStateChangedMessage>>()))
            .Callback<object, MessageHandler<object, GraphStateChangedMessage>>((_, action)
                => handler = action);

        var viewModel = mock.Create<GraphFieldViewModel>();

        var enabled = await viewModel.ChangeVertexPolarityCommand.CanExecute.FirstAsync(value => value);

        handler?.Invoke(viewModel, new GraphStateChangedMessage((6, GraphStatuses.Readonly)));

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
        using var mock = AutoMock.GetLoose();

        var vertex = CreateVertex();
        var graph = new Graph<GraphVertexModel>([vertex], 1);
        var activatedMessage = new GraphActivatedMessage(new ActivatedGraphModel(
            graph,
            default,
            default,
            GraphStatuses.Editable,
            7));

        MessageHandler<object, GraphsDeletedMessage>? handler = null;

        mock.Mock<IMessenger>()
            .Setup(x => x.Register(
                It.IsAny<object>(),
                It.Is<int>(token => token == Tokens.GraphField),
                It.IsAny<MessageHandler<object, GraphActivatedMessage>>()))
            .Callback<object, int, MessageHandler<object, GraphActivatedMessage>>((recipient, _, action)
                => action(recipient, activatedMessage));

        mock.Mock<IMessenger>()
            .Setup(x => x.Register(
                It.IsAny<object>(),
                It.IsAny<MessageHandler<object, GraphsDeletedMessage>>()))
            .Callback<object, MessageHandler<object, GraphsDeletedMessage>>((_, action)
                => handler = action);

        var viewModel = mock.Create<GraphFieldViewModel>();

        var enabled = await viewModel.ChangeVertexPolarityCommand.CanExecute.FirstAsync(value => value);
        Assert.That(enabled, Is.True);

        handler?.Invoke(viewModel, new GraphsDeletedMessage([7]));

        var disabled = await viewModel.ChangeVertexPolarityCommand.CanExecute.FirstAsync(value => !value);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.Graph, Is.SameAs(Graph<GraphVertexModel>.Empty));
            Assert.That(disabled, Is.False);
        });
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
