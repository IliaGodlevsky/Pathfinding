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

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class GraphFieldView : FrameView
    {
        public const int DistanceBetweenVertices = 3;

        private readonly IGraphFieldViewModel graphFieldViewModel;
        private readonly IRunRangeViewModel runRangeViewModel;

        private readonly CompositeDisposable disposables = [];
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
                .Subscribe()
                .DisposeWith(disposables);
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

            var views = new List<GraphVertexView>();

            foreach (var vertex in graph)
            {
                var view = new GraphVertexView(vertex);
                view.DisposeWith(vertexDisposables);
                SubscribeToButton1Clicked(view, vertex);
                SubscribeToButton3Clicked(view, vertex);
                SubscribeOnWheelButton(view, vertex);
                views.Add(view);
            }

            Application.MainLoop.Invoke(() =>
            {
                container.Add([.. views]);
                container.Width = graph.GetWidth() * DistanceBetweenVertices;
                container.Height = graph.GetLength();
            });
        }

        private void SubscribeToButton1Clicked(GraphVertexView view, GraphVertexModel model)
        {
            view.Events().MouseClick
                .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Pressed)
                .Select(x => model)
                .InvokeCommand(runRangeViewModel, x => x.AddToRangeCommand)
                .DisposeWith(vertexDisposables);

            view.Events().MouseClick
                .Where(x => x.MouseEvent.Flags.HasFlag(MouseFlags.Button1Pressed)
                       && x.MouseEvent.Flags.HasFlag(MouseFlags.ButtonCtrl))
                .Select(x => model)
                .InvokeCommand(runRangeViewModel, x => x.RemoveFromRangeCommand)
                .DisposeWith(vertexDisposables);

            view.Events().MouseClick
                .Where(x => x.MouseEvent.Flags.HasFlag(MouseFlags.Button1DoubleClicked)
                       && x.MouseEvent.Flags.HasFlag(MouseFlags.ButtonCtrl)
                       && x.MouseEvent.Flags.HasFlag(MouseFlags.ButtonAlt))
                .Select(x => Unit.Default)
                .InvokeCommand(runRangeViewModel, x => x.DeletePathfindingRange)
                .DisposeWith(vertexDisposables);
        }

        private void SubscribeToButton3Clicked(GraphVertexView view, GraphVertexModel model)
        {
            view.Events().MouseClick
                .Where(x => x.MouseEvent.Flags.HasFlag(MouseFlags.Button3Pressed)
                         && x.MouseEvent.Flags.HasFlag(MouseFlags.ButtonCtrl))
                .Select(x => model)
                .InvokeCommand(graphFieldViewModel, x => x.ReverseVertexCommand)
                .DisposeWith(vertexDisposables);
            view.Events().MouseClick
                .Where(x => x.MouseEvent.Flags.HasFlag(MouseFlags.Button3Pressed)
                         && x.MouseEvent.Flags.HasFlag(MouseFlags.ButtonAlt))
                .Select(x => model)
                .InvokeCommand(graphFieldViewModel, x => x.InverseVertexCommand)
                .DisposeWith(vertexDisposables);
            view.Events().MouseClick
                .Where(x => x.MouseEvent.Flags.HasFlag(MouseFlags.Button3Clicked))
                .Select(x => model)
                .InvokeCommand(graphFieldViewModel, x => x.ChangeVertexPolarityCommand)
                .DisposeWith(vertexDisposables);
        }

        private void SubscribeOnWheelButton(GraphVertexView view, GraphVertexModel model)
        {
            view.Events().MouseClick
                .Where(x => x.MouseEvent.Flags.HasFlag(MouseFlags.WheeledDown))
                .Select(x => model)
                .InvokeCommand(graphFieldViewModel, x => x.DecreaseVertexCostCommand)
                .DisposeWith(vertexDisposables);
            view.Events().MouseClick
                .Where(x => x.MouseEvent.Flags.HasFlag(MouseFlags.WheeledUp))
                .Select(x => model)
                .InvokeCommand(graphFieldViewModel, x => x.IncreaseVertexCostCommand)
                .DisposeWith(vertexDisposables);
        }
    }
}
