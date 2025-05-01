using Pathfinding.App.Console.ViewModels;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphNameUpdateView : FrameView
{
    public GraphNameUpdateView(GraphUpdateViewModel viewModel)
    {
        Initialize();
        nameField.Events().TextChanged.Select(_ => nameField.Text)
            .BindTo(viewModel, x => x.Name);
        viewModel.WhenAnyValue(x => x.Name)
            .Where(x => x != null)
            .Do(x => nameField.Text = x)
            .Subscribe();
    }
}
