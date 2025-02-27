using Pathfinding.App.Console.ViewModels;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class GraphNameUpdateView : FrameView
    {
        private readonly GraphUpdateViewModel viewModel;
        private readonly CompositeDisposable disposables = [];

        public GraphNameUpdateView(GraphUpdateViewModel viewModel)
        {
            Initialize();
            this.viewModel = viewModel;
            nameField.Events().TextChanged.Select(_ => nameField.Text)
                .BindTo(this.viewModel, x => x.Name)
                .DisposeWith(disposables);
            viewModel.WhenAnyValue(x => x.Name)
                .Where(x => x != null)
                .Do(x => nameField.Text = x)
                .Subscribe()
                .DisposeWith(disposables);
        }
    }
}
