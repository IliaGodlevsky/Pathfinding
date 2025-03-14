using Pathfinding.App.Console.Resources;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed class RunUpdateView : Button
    {
        public RunUpdateView(IRunUpdateViewModel viewModel)
        {
            X = Pos.Percent(33);
            Y = 0;
            Width = Dim.Percent(33);
            Text = Resource.UpdateAlgorithms;
            viewModel.UpdateRunsCommand.CanExecute
                .BindTo(this, x => x.Enabled);
            this.Events().MouseClick
                .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
                .Throttle(TimeSpan.FromMilliseconds(30))
                .Select(x => Unit.Default)
                .InvokeCommand(viewModel, x => x.UpdateRunsCommand);
        }
    }
}
