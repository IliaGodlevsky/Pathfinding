using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Factories;
using Pathfinding.App.Console.Messages.ViewModel.Requests;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.App.Console.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class RunFieldViewModelTests
{
    [Test]
    public async Task AwaitGraphActivatedMessage_ShouldAssembleRunGraph()
    {
        var messenger = new StrongReferenceMessenger();
        var graphAssemble = new GraphAssemble<RunVertexModel>();
        var algorithmsFactoryMock = CreateAlgorithmsFactoryMock();

        using var viewModel = new RunFieldViewModel(
            graphAssemble,
            algorithmsFactoryMock.Object,
            messenger);

        var graph = CreateGraph();
        var message = new AwaitGraphActivatedMessage(new ActivatedGraphModel(
            new(1, graph, false),
            default,
            default));
        await messenger.Send(message);

        Assert.That(viewModel.RunGraph, Is.Not.EqualTo(Graph<RunVertexModel>.Empty));
    }

    [Test]
    public async Task RunsSelectedMessage_ShouldActivateRun()
    {
        var messenger = new StrongReferenceMessenger();
        var graphAssemble = new GraphAssemble<RunVertexModel>();
        var algorithmsFactoryMock = CreateAlgorithmsFactoryMock();

        using var viewModel = new RunFieldViewModel(
            graphAssemble,
            algorithmsFactoryMock.Object,
            messenger);

        var graph = CreateGraph();
        var message = new AwaitGraphActivatedMessage(new ActivatedGraphModel(
            new(42, graph, false),
            default,
            default));
        await messenger.Send(message);

        messenger.Register<PathfindingRangeRequestMessage>(this, (_, msg)
            => msg.Reply([.. graph]));

        var runInfo = new RunInfoModel { Id = 7, Algorithm = Algorithms.AStar };
        messenger.Send(new RunsSelectedMessage([runInfo]));

        Assert.That(viewModel.SelectedRun.Id, Is.EqualTo(runInfo.Id));
    }

    [Test]
    public async Task RunsDeletedMessage_ShouldClearSelectedRun()
    {
        var messenger = new StrongReferenceMessenger();
        var graphAssemble = new GraphAssemble<RunVertexModel>();
        var algorithmsFactoryMock = CreateAlgorithmsFactoryMock();

        using var viewModel = new RunFieldViewModel(
            graphAssemble,
            algorithmsFactoryMock.Object,
            messenger);

        var graph = CreateGraph();
        var message = new AwaitGraphActivatedMessage(new ActivatedGraphModel(
            new(42, graph, false),
            default,
            default));
        await messenger.Send(message);

        messenger.Register<PathfindingRangeRequestMessage>(this, (_, msg)
            => msg.Reply([.. graph]));

        var runInfo = new RunInfoModel { Id = 4, Algorithm = Algorithms.AStar };
        messenger.Send(new RunsSelectedMessage([runInfo]));

        messenger.Send(new RunsDeletedMessage([runInfo.Id]));

        Assert.That(viewModel.SelectedRun, Is.EqualTo(RunModel.Empty));
    }

    private static Graph<GraphVertexModel> CreateGraph()
    {
        var first = new GraphVertexModel { Position = new Coordinate(0), Cost = new VertexCost(1, (1, 2)) };
        var second = new GraphVertexModel { Position = new Coordinate(1), Cost = new VertexCost(3, (2, 4)) };
        first.Neighbors.Add(second);
        second.Neighbors.Add(first);
        return new Graph<GraphVertexModel>([first, second], [2]);
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
}
