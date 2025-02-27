using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed class GraphCopyButton : Button
    {
        private readonly CompositeDisposable disposables = [];

        public GraphCopyButton(IGraphCopyViewModel viewModel)
        {
            X = Pos.Percent(33.34f);
            Y = 0;
            Width = Dim.Percent(16.67f);
            Text = "Copy";
            viewModel.CopyGraphCommand.CanExecute
                .BindTo(this, x => x.Enabled)
                .DisposeWith(disposables);
            this.Events().MouseClick
                .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
                .Select(x => Unit.Default)
                .InvokeCommand(viewModel, x => x.CopyGraphCommand)
                .DisposeWith(disposables);
        }
    }
}
