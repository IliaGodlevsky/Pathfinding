using NStack;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphNeighborhoodView : FrameView
{
    public GraphNeighborhoodView(IRequireNeighborhoodNameViewModel viewModel)
    {
        var map = viewModel.AllowedNeighborhoods
            .ToDictionary(x => x.ToStringRepresentation());
        Initialize();
        var labels = map.Keys.Select(ustring.Make).ToArray();
        var values = labels.Select(x => map[x.ToString()]).ToList();
        this.neighborhoods.RadioLabels = labels;
        this.neighborhoods.Events().SelectedItemChanged
            .Where(x => x.SelectedItem > -1)
            .Select(x => values[x.SelectedItem])
            .BindTo(viewModel, x => x.Neighborhood);
        this.neighborhoods.SelectedItem = 0;
        this.Events().VisibleChanged
            .Where(_ => Visible)
            .Do(_ => neighborhoods.SelectedItem = 0);
    }
}
