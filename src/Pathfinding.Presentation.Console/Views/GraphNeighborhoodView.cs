using NStack;
using Pathfinding.Presentation.Console.Extensions;
using Pathfinding.Presentation.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class GraphNeighborhoodView : FrameView
{
    private readonly CompositeDisposable disposables = [];

    public GraphNeighborhoodView(IRequireNeighborhoodNameViewModel viewModel)
    {
        var map = viewModel.AllowedNeighborhoods
            .ToDictionary(x => x.ToStringRepresentation());
        Initialize();
        var labels = map.Keys.Select(ustring.Make).ToArray();
        var values = labels.Select(x => map[x.ToString()]).ToList();
        neighborhoods.RadioLabels = labels;
        neighborhoods.Events().SelectedItemChanged
            .Where(x => x.SelectedItem > -1 && values.Count > 0)
            .Select(x => values[x.SelectedItem])
            .BindTo(viewModel, x => x.Neighborhood)
            .DisposeWith(disposables);
        neighborhoods.SelectedItem = 0;
        this.Events().VisibleChanged
            .Where(_ => Visible)
            .Do(_ => neighborhoods.SelectedItem = 0)
            .Subscribe()
            .DisposeWith(disposables);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}
