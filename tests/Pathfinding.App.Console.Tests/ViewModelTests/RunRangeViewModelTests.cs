using Autofac.Features.Metadata;
using CommunityToolkit.Mvvm.Messaging;
using Moq;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Business.Commands;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Logging.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Shared.Primitives;
using System.Reactive.Linq;
using System.Linq;
using System.Threading.Tasks;
using Command = Pathfinding.Service.Interface.IPathfindingRangeCommand<Pathfinding.App.Console.Models.GraphVertexModel>;

namespace Pathfinding.App.Console.Tests.ViewModelTests;

[Category("Unit")]
internal sealed class RunRangeViewModelTests
{
    [Test]
    public async Task AddToRangeCommand_ShouldExecuteFirstAvailableCommand()
    {
        var messenger = new StrongReferenceMessenger();
        var rangeServiceMock = new Mock<IRangeRequestService<GraphVertexModel>>();
        rangeServiceMock
            .Setup(x => x.ReadRangeAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PathfindingRangeModel>());
        var includeCommands = new[]
        {
            CreateCommandMeta(new StubCommand(canExecute: false)),
            CreateCommandMeta(new IncludeSourceVertex<GraphVertexModel>(), order: 2)
        };
        var excludeCommands = Array.Empty<Meta<Command>>();

        using var viewModel = CreateViewModel(
            messenger,
            rangeServiceMock,
            includeCommands,
            excludeCommands);

        var graph = CreateGraph();
        await ActivateGraphAsync(messenger, graph);

        var vertex = graph.First();

        viewModel.AddToRangeCommand.Execute(vertex);

        Assert.That(viewModel.Source, Is.EqualTo(vertex));
    }

    [Test]
    public async Task RemoveFromRangeCommand_ShouldExecuteFirstAvailableCommand()
    {
        var messenger = new StrongReferenceMessenger();
        var rangeServiceMock = new Mock<IRangeRequestService<GraphVertexModel>>();
        rangeServiceMock
            .Setup(x => x.ReadRangeAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PathfindingRangeModel>());
        var includeCommands = new[]
        {
            CreateCommandMeta(new IncludeSourceVertex<GraphVertexModel>())
        };
        var excludeCommands = new[]
        {
            CreateCommandMeta(new StubCommand(canExecute: false)),
            CreateCommandMeta(new ExcludeSourceVertex<GraphVertexModel>(), order: 2)
        };

        using var viewModel = CreateViewModel(
            messenger,
            rangeServiceMock,
            includeCommands,
            excludeCommands);

        var graph = CreateGraph();
        await ActivateGraphAsync(messenger, graph);

        var vertex = graph.First();
        viewModel.AddToRangeCommand.Execute(vertex);
        viewModel.RemoveFromRangeCommand.Execute(vertex);

        Assert.That(viewModel.Source, Is.Null);
    }

    [Test]
    public async Task DeletePathfindingRange_ShouldClearStoredRangeAsync()
    {
        var messenger = new StrongReferenceMessenger();
        var rangeServiceMock = new Mock<IRangeRequestService<GraphVertexModel>>();
        rangeServiceMock
            .Setup(x => x.ReadRangeAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new() { Position = new Coordinate(0), IsSource = true },
                new() { Position = new Coordinate(1), IsTarget = true }
            ]);

        rangeServiceMock
            .Setup(x => x.DeleteRangeAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var includeCommands = new[]
        {
            CreateCommandMeta(new StubCommand(canExecute: true,
                execute: (range, vertex) =>
                {
                    if (range.Source == null) range.Source = vertex;
                    else range.Target = vertex;
                }))
        };
        var excludeCommands = Array.Empty<Meta<Command>>();

        using var viewModel = CreateViewModel(
            messenger,
            rangeServiceMock,
            includeCommands,
            excludeCommands);

        var graph = CreateGraph();
        await ActivateGraphAsync(messenger, graph, graphId: 12);

        Assert.Multiple(() =>
        {
            Assert.That(viewModel.Source, Is.Not.Null);
            Assert.That(viewModel.Target, Is.Not.Null);
        });

        await viewModel.DeletePathfindingRange.Execute();

        Assert.Multiple(() =>
        {
            rangeServiceMock.Verify(x => x.DeleteRangeAsync(12, It.IsAny<CancellationToken>()), Times.Once);
            Assert.That(viewModel.Source, Is.Null);
            Assert.That(viewModel.Target, Is.Null);
            Assert.That(viewModel.Transit, Is.Empty);
        });
    }

    private static Task ActivateGraphAsync(
        IMessenger messenger,
        Graph<GraphVertexModel> graph,
        int graphId = 12,
        GraphStatuses status = GraphStatuses.Editable)
    {
        return messenger.Send(new AwaitGraphActivatedMessage(new ActivatedGraphModel(
            graph,
            Neighborhoods.Moore,
            SmoothLevels.No,
            status,
            GraphId: graphId)), Tokens.PathfindingRange);
    }

    private static RunRangeViewModel CreateViewModel(
        IMessenger messenger,
        Mock<IRangeRequestService<GraphVertexModel>> rangeServiceMock,
        Meta<Command>[] includeCommands,
        Meta<Command>[] excludeCommands,
        ILog log = null)
    {
        return new RunRangeViewModel(
            messenger,
            includeCommands,
            excludeCommands,
            rangeServiceMock.Object,
            log ?? Mock.Of<ILog>());
    }

    private static Graph<GraphVertexModel> CreateGraph()
    {
        var first = new GraphVertexModel
        {
            Position = new Coordinate(0),
            Cost = new StubVertexCost()
        };
        var second = new GraphVertexModel
        {
            Position = new Coordinate(1),
            Cost = new StubVertexCost()
        };
        first.Neighbors.Add(second);
        second.Neighbors.Add(first);
        return new Graph<GraphVertexModel>([first, second], [2]);
    }

    private static Meta<Command> CreateCommandMeta(Command command, int order = 1)
    {
        var metadata = new Dictionary<string, object>
        {
            [MetadataKeys.Order] = order
        };
        return new Meta<Command>(command, metadata);
    }

    private sealed class StubCommand(bool canExecute, Action<IPathfindingRange<GraphVertexModel>, GraphVertexModel> execute = null) : Command
    {
        private readonly Action<IPathfindingRange<GraphVertexModel>, GraphVertexModel> execute = execute ?? ((_, _) => { });

        public void Execute(IPathfindingRange<GraphVertexModel> range, GraphVertexModel vertex)
        {
            execute(range, vertex);
        }

        public bool CanExecute(IPathfindingRange<GraphVertexModel> range, GraphVertexModel vertex)
        {
            return canExecute;
        }
    }

    private sealed class StubVertexCost : IVertexCost
    {
        public InclusiveValueRange<int> CostRange { get; set; } = new(0, 0);

        public int CurrentCost { get; set; }

        public IVertexCost Clone()
        {
            return new StubVertexCost
            {
                CostRange = CostRange,
                CurrentCost = CurrentCost
            };
        }
    }
}
