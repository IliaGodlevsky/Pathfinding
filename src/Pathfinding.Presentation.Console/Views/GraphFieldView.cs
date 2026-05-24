using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.Data;
using Pathfinding.Data.Extensions;
using Pathfinding.Domain.Interface;
using Pathfinding.Presentation.Console.Extensions;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Messages.View;
using Pathfinding.Presentation.Console.Models;
using Pathfinding.Presentation.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;
using static Terminal.Gui.MouseFlags;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class GraphFieldView : FrameView
{
    public const int DistanceBetweenVertices = 3;

    private readonly IGraphFieldViewModel graphFieldViewModel;
    private readonly IRunRangeViewModel runRangeViewModel;
    private readonly IMessenger messenger;

    private readonly CompositeDisposable disposables = [];
    private readonly CompositeDisposable vertexDisposables = [];
    private readonly MainLoop mainLoop;

    private readonly List<View> containers = [];

    private View currentContainer;

    public GraphFieldView(
        IGraphFieldViewModel graphFieldViewModel,
        IRunRangeViewModel runRangeViewModel,
        [KeyFilter(KeyFilters.Views)] IMessenger messenger)
    {
        mainLoop = Application.MainLoop;
        this.messenger = messenger;
        this.graphFieldViewModel = graphFieldViewModel;
        this.runRangeViewModel = runRangeViewModel;
        Initialize();
        this.graphFieldViewModel.WhenAnyValue(x => x.ActivatedGraph)
            .DistinctUntilChanged(x => x.Id)
            .Subscribe(x => RenderGraph(x.Graph))
            .DisposeWith(disposables);
        messenger.RegisterHandler<OpenRunFieldMessage>(this, OnOpenAlgorithmRunView).DisposeWith(disposables);
        messenger.RegisterHandler<CloseRunFieldMessage>(this, OnCloseAlgorithmRunField).DisposeWith(disposables);
        messenger.RegisterHandler<SliceChangedMessage>(this, OnSliceChanged).DisposeWith(disposables);
        vertexDisposables.DisposeWith(disposables);
    }

    private void OnSliceChanged(SliceChangedMessage msg)
    {
        if (currentContainer != null)
        {
            Remove(currentContainer);
            currentContainer = containers[msg.Slice - 1];
            Add(currentContainer);
        }
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }

    private void OnOpenAlgorithmRunView(OpenRunFieldMessage msg)
    {
        mainLoop.Invoke(() => Visible = false);
    }

    private void OnCloseAlgorithmRunField(CloseRunFieldMessage msg)
    {
        mainLoop.Invoke(() => Visible = true);
    }

    private void RenderGraph(Graph<GraphVertexModel> graph)
    {
        mainLoop.Invoke(() =>
        {
            containers.ForEach(x => x.RemoveAll());
            containers.ForEach(Remove);
            containers.Clear();
        });
        
        vertexDisposables.Clear();
        var views = graph.AsParallel().Select(x =>
        {
            var view = new GraphVertexView(x).DisposeWith(vertexDisposables);

            BindTo(view, x, runRangeViewModel.AddToRangeCommand, Button1Pressed);
            BindTo(view, x, runRangeViewModel.RemoveFromRangeCommand, Button1Pressed | ButtonCtrl);
            BindTo(view, runRangeViewModel.DeletePathfindingRange, Button1DoubleClicked | ButtonCtrl | ButtonAlt);

            BindTo(view, x, graphFieldViewModel.ReverseVertexCommand, Button3Pressed | ButtonCtrl);
            BindTo(view, x, graphFieldViewModel.InverseVertexCommand, Button3Pressed | ButtonAlt);
            BindTo(view, x, graphFieldViewModel.ChangeVertexPolarityCommand, Button3Clicked);

            BindTo(view, x, graphFieldViewModel.DecreaseVertexCostCommand, WheeledDown);
            BindTo(view, x, graphFieldViewModel.IncreaseVertexCostCommand, WheeledUp);
            return view;
        }).ToArray();

        mainLoop.Invoke(() =>
        {
            int i = 0;
            do
            {
                var container = new View
                {
                    X = Pos.Center(),
                    Y = Pos.Center(),
                    Width = graph.GetWidth() * DistanceBetweenVertices,
                    Height = graph.GetLength()
                };
                container.Add([.. views.Where(x => ((IVertex)x.Data).Position.ElementAtOrDefault(2) == i)]);
                containers.Add(container);
                i++;
            } while (i < graph.GetDepth());
            if (containers.Count > 0)
            {
                currentContainer = containers[0];
                Add(currentContainer);
                messenger.Send(new GraphActivatedMessage(graph));
            }
        });
    }

    private void BindTo<T>(GraphVertexView view, T model,
        ReactiveCommand<T, Unit> command,
        params MouseFlags[] flags)
    {
        view.Events().MouseClick
            .Where(x => flags.Any(z => x.MouseEvent.Flags.HasFlag(z)))
            .Select(_ => model)
            .InvokeCommand(command)
            .DisposeWith(vertexDisposables);
    }

    private void BindTo(GraphVertexView view,
        ReactiveCommand<Unit, Unit> command,
        params MouseFlags[] flags)
    {
        BindTo(view, Unit.Default, command, flags);
    }
}
