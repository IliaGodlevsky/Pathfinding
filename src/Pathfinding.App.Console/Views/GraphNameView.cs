using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class GraphNameView : FrameView
    {
        private readonly CompositeDisposable disposables = [];

        public GraphNameView(IRequireGraphNameViewModel viewModel)
        {
            Initialize();
            nameField.Events().TextChanged
                .Select(_ => nameField.Text)
                .BindTo(viewModel, x => x.Name)
                .DisposeWith(disposables);
            this.Events().VisibleChanged
                .Where(x => Visible)
                .Do(x => nameField.Text = string.Empty)
                .Subscribe()
                .DisposeWith(disposables);
        }
    }
}
