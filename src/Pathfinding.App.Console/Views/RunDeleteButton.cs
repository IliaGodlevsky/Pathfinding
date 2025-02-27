using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class RunDeleteButton : Button
    {
        private readonly CompositeDisposable disposables = [];

        public RunDeleteButton(IRunDeleteViewModel viewModel)
        {
            Initialize();
            viewModel.DeleteRunsCommand.CanExecute
                .BindTo(this, x => x.Enabled)
                .DisposeWith(disposables);
            this.Events().MouseClick
                .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
                .Select(x => Unit.Default)
                .InvokeCommand(viewModel, x => x.DeleteRunsCommand)
                .DisposeWith(disposables);
        }
    }
}
