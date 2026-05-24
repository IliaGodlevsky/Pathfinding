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
            .DistinctUntilChanged(x => x != Graph<RunVertexModel>.Empty)
            .Where(x => x is not null)
            .Subscribe(RenderGraphState)
            .DisposeWith(disposables);
        messenger.RegisterHandler<OpenRunFieldMessage>(this, OnOpen).DisposeWith(disposables);
        messenger.RegisterHandler<CloseRunFieldMessage>(this, OnClose).DisposeWith(disposables);
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
        mainLoop.Invoke(()=>
        {
            foreach(var container in containers)
            {
                container.RemoveAll();
                Remove(container);
            }
            containers.Clear();
        });
        vertexDisposables.Clear();
        var children = graph
            .Select(x => new RunVertexView(x).DisposeWith(vertexDisposables))
            .ToArray();
        mainLoop.Invoke(() =>
        {
            int i = 0;
            do
            {
                var container = new View()
                {
                    X = Pos.Center(),
                    Y = Pos.Center(),
                    Width = graph.GetWidth() * GraphFieldView.DistanceBetweenVertices,
                    Height = graph.GetLength()
                };
                container.Add([.. children.Where(x => ((IVertex)x.Data).Position.ElementAtOrDefault(2) == i)]);
                containers.Add(container);
                i++;
            } while (i < graph.GetDepth());
            if (containers.Count > 0 && children.Length > 0)
            {
                currentContainer = containers[0];
                Add(currentContainer);
            }
        });
    }
}
