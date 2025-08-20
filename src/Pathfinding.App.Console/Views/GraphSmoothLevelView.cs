using NStack;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphSmoothLevelView : FrameView
{
    private readonly CompositeDisposable disposables = [];

    public GraphSmoothLevelView(IRequireSmoothLevelViewModel viewModel)
    {
        var smoothLevels = viewModel.AllowedLevels
            .ToDictionary(x => x.ToStringRepresentation());
        Initialize();
        var labels = smoothLevels.Keys.Select(ustring.Make).ToArray();
        var values = labels.Select(x => smoothLevels[x.ToString()]).ToList();
        this.smoothLevels.RadioLabels = labels;
        this.smoothLevels.Events()
            .SelectedItemChanged
            .Where(x => x.SelectedItem > -1)
            .Select(x => values[x.SelectedItem])
            .BindTo(viewModel, x => x.SmoothLevel)
            .DisposeWith(disposables);
        this.smoothLevels.SelectedItem = 0;
        VisibleChanged += OnVisibilityChanged;
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        VisibleChanged -= OnVisibilityChanged;
        base.Dispose(disposing);
    }

    private void OnVisibilityChanged()
    {
        if (Visible)
        {
            smoothLevels.SelectedItem = 0;
        }
    }
}
