using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.Data;
using Pathfinding.Data.Extensions;
using Pathfinding.Domain.Interface;
using Pathfinding.Presentation.Console.Extensions;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Messages.View;
using Pathfinding.Presentation.Console.Models;
using Pathfinding.Presentation.Console.Resources;
using Pathfinding.Presentation.Console.ViewModels.Interface;
using Pathfinding.Service.Extensions;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class RunFieldView : FrameView
{
    private readonly CompositeDisposable vertexDisposables = [];
    private readonly CompositeDisposable disposables = [];
    private readonly MainLoop mainLoop = Application.MainLoop;
    private readonly List<View> containers = [];

    private int Slice { get; set; } = 1;

    private View currentContainer;

    public RunFieldView(IRunFieldViewModel viewModel,
        [KeyFilter(KeyFilters.Views)] IMessenger messenger)
    {
        Visible = false;
        X = 0;
        Y = Pos.Percent(7);
        Width = Dim.Percent(66);
        Height = Dim.Percent(90);
        Border = new()
        {
            BorderBrush = Color.BrightYellow,
            BorderStyle = BorderStyle.Rounded,
            Title = Resource.RunField
        };
        viewModel.WhenAnyValue(x => x.RunGraph)
            .DistinctUntilChanged()
            .Where(x => x != Graph<RunVertexModel>.Empty)
            .Subscribe(RenderGraphState)
            .DisposeWith(disposables);
        messenger.RegisterHandler<OpenRunFieldMessage>(this, OnOpen).DisposeWith(disposables);
        messenger.RegisterHandler<CloseRunFieldMessage>(this, OnClose).DisposeWith(disposables);
        messenger.RegisterHandler<SliceChangedMessage>(this, OnSliceChanged).DisposeWith(disposables);
        vertexDisposables.DisposeWith(disposables);
    }

    private void OnSliceChanged(SliceChangedMessage msg)
    {
        Slice = msg.Slice;
        if (currentContainer != null)
        {
            Remove(currentContainer);
            currentContainer = containers.ElementAtOrDefault(Slice - 1);
            Add(currentContainer);
        }
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }

    private void OnOpen(OpenRunFieldMessage msg)
    {
        mainLoop.Invoke(() => Visible = true);
    }

    private void OnClose(CloseRunFieldMessage msg)
    {
        mainLoop.Invoke(() => Visible = false);
    }

    private void RenderGraphState(IGraph<RunVertexModel> graph)
    {
        mainLoop.Invoke(() =>
        {
            containers.ForEach(container =>
            {
                container.RemoveAll();
                Remove(container);
            });
            containers.Clear();
        });
        vertexDisposables.Clear();
        var children = graph
            .AsParallel()
            .Select(x => new RunVertexView(x).DisposeWith(vertexDisposables))
            .ToArray();
        mainLoop.Invoke(() =>
        {
            for (int i = 0; i < graph.GetDepth(); i++)
            {
                var container = CreateContainer(graph);
                container.Add([.. children.Where(x => x.Model.GetZ() == i)]);
                containers.Add(container);
            }
            if (containers.Count > 0)
            {
                Remove(currentContainer);
                currentContainer = containers.ElementAtOrDefault(Slice - 1);
                Add(currentContainer);
            }
        });
    }

    private static View CreateContainer(IGraph<RunVertexModel> graph)
    {
        return new View()
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Width = graph.GetWidth() * GraphFieldView.DistanceBetweenVertices,
            Height = graph.GetLength()
        };
    }
}
