using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Data.Extensions;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

using static Terminal.Gui.MouseFlags;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphFieldView : FrameView
{
    public const int DistanceBetweenVertices = 3;

    private readonly IGraphFieldViewModel graphFieldViewModel;
    private readonly IRunRangeViewModel runRangeViewModel;

    private readonly CompositeDisposable vertexDisposables = [];

    private readonly View container = new();

    public GraphFieldView(
        IGraphFieldViewModel graphFieldViewModel,
        IRunRangeViewModel runRangeViewModel,
        [KeyFilter(KeyFilters.Views)] IMessenger messenger)
    {
        this.graphFieldViewModel = graphFieldViewModel;
        this.runRangeViewModel = runRangeViewModel;
        Initialize();
        this.graphFieldViewModel.WhenAnyValue(x => x.Graph)
            .Where(x => x != null)
            .Do(RenderGraph)
            .Subscribe();
        messenger.Register<OpenRunFieldMessage>(this, OnOpenAlgorithmRunView);
        messenger.Register<CloseRunFieldMessage>(this, OnCloseAlgorithmRunField);
        container.X = Pos.Center();
        container.Y = Pos.Center();
        Add(container);
    }

    private void OnOpenAlgorithmRunView(object recipient, OpenRunFieldMessage msg)
    {
        Application.MainLoop.Invoke(() => Visible = false);
    }

    private void OnCloseAlgorithmRunField(object recipient, CloseRunFieldMessage msg)
    {
        Application.MainLoop.Invoke(() => Visible = true);
    }

    private void RenderGraph(IGraph<GraphVertexModel> graph)
    {
        Application.MainLoop.Invoke(container.RemoveAll);
        vertexDisposables.Clear();

        foreach (var vertex in graph)
        {
            var view = new GraphVertexView(vertex).DisposeWith(vertexDisposables);

            BindTo(view, vertex, runRangeViewModel.AddToRangeCommand, Button1Pressed);
            BindTo(view, vertex, runRangeViewModel.RemoveFromRangeCommand,
                Button1Pressed | ButtonCtrl);
            BindTo(view, runRangeViewModel.DeletePathfindingRange, 
                Button1DoubleClicked | ButtonCtrl | ButtonAlt);

            BindTo(view, vertex, graphFieldViewModel.ReverseVertexCommand, Button3Pressed | ButtonCtrl);
            BindTo(view, vertex, graphFieldViewModel.InverseVertexCommand, Button3Pressed | ButtonAlt);
            BindTo(view, vertex, graphFieldViewModel.ChangeVertexPolarityCommand, Button3Clicked);

            BindTo(view, vertex, graphFieldViewModel.DecreaseVertexCostCommand, WheeledDown);
            BindTo(view, vertex, graphFieldViewModel.IncreaseVertexCostCommand, WheeledUp);

            Application.MainLoop.Invoke(() => container.Add(view));
        }

        Application.MainLoop.Invoke(() =>
        {
            container.Width = graph.GetWidth() * DistanceBetweenVertices;
            container.Height = graph.GetLength();
        });
    }

    private void BindTo<T>(GraphVertexView view, T model,
        ReactiveCommand<T, Unit> command, 
        params MouseFlags[] flags)
    {
        view.Events().MouseClick
            .Where(x => flags.Any(z => x.MouseEvent.Flags.HasFlag(z)))
            .Select(x => model)
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
