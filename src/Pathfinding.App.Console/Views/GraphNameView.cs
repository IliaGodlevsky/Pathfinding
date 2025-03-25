using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphNameView : FrameView
{
    public GraphNameView(IRequireGraphNameViewModel viewModel)
    {
        Initialize();
        nameField.Events().TextChanged
            .Select(_ => nameField.Text)
            .BindTo(viewModel, x => x.Name);
        this.Events().VisibleChanged
            .Where(x => Visible)
            .Do(x => nameField.Text = string.Empty)
            .Subscribe();
    }
}
