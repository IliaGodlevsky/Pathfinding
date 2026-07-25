using Autofac.Features.AttributeFilters;
using Pathfinding.Presentation.Console.Injection;
using ReactiveMarbles.ObservableEvents;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class RunsPanel : FrameView
{
    private readonly CompositeDisposable disposables = [];

    public RunsPanel([KeyFilter(KeyFilters.RunsPanel)] View[] children)
    {
        Initialize();
        Add(children);

        var table = children.OfType<RunsTableView>().Single();
        var filterLabel = new Label("Filter:")
        {
            X = 0,
            Y = 0,
            Width = 7
        };
        var filterInput = new TextField()
        {
            X = Pos.Right(filterLabel),
            Y = 0,
            Width = Dim.Fill()
        };
        filterInput.Events().TextChanging
            .Subscribe(args => table.ApplyFilter(args.NewText.ToString()))
            .DisposeWith(disposables);

        table.Y = 1;
        Add(filterLabel, filterInput);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}
