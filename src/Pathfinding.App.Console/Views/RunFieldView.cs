using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.Resources;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Data.Extensions;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunFieldView : FrameView
{
    private readonly CompositeDisposable vertexDisposables = [];
    private readonly CompositeDisposable disposables = [];

    private readonly View container = new();

    public RunFieldView(IRunFieldViewModel viewModel,
        [KeyFilter(KeyFilters.Views)] IMessenger messenger)
    {
        Visible = false;
        X = 0;
        Y = 0;
        Width = Dim.Percent(66);
        Height = Dim.Percent(95);
        Border = new()
        {
            BorderBrush = Color.BrightYellow,
            BorderStyle = BorderStyle.Rounded,
            Title = Resource.RunField
        };
        container.X = Pos.Center();
        container.Y = Pos.Center();
        viewModel.WhenAnyValue(x => x.RunGraph)
            .DistinctUntilChanged()
            .Where(x => x is not null)
            .Do(async x => await RenderGraphState(x))
            .Subscribe()
            .DisposeWith(disposables);
        messenger.RegisterHandler<OpenRunFieldMessage>(this, OnOpen).DisposeWith(disposables);
        messenger.RegisterHandler<CloseRunFieldMessage>(this, OnClose).DisposeWith(disposables);
        Add(container);
        vertexDisposables.DisposeWith(disposables);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }

    private void OnOpen(OpenRunFieldMessage msg)
    {
        Application.MainLoop.Invoke(() => Visible = true);
    }

    private void OnClose(CloseRunFieldMessage msg)
    {
        Application.MainLoop.Invoke(() => Visible = false);
    }

    private async Task RenderGraphState(IGraph<RunVertexModel> graph)
    {
        Application.MainLoop.Invoke(container.RemoveAll);
        vertexDisposables.Clear();
        var children = new List<RunVertexView>(graph.Count);
        await Task.Run(() =>
        {
            foreach (var vertex in graph)
            {
                var view = new RunVertexView(vertex);
                children.Add(view);
                view.DisposeWith(vertexDisposables);
            }
        });
        Application.MainLoop.Invoke(() =>
        {
            container.Add([.. children]);
            container.Width = graph.GetWidth()
                * GraphFieldView.DistanceBetweenVertices;
            container.Height = graph.GetLength();
        });
    }
}
