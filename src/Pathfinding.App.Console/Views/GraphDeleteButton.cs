using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class GraphDeleteButton : Button
    {
        public GraphDeleteButton(IGraphDeleteViewModel viewModel)
        {
            Initialize();
            viewModel.DeleteGraphCommand.CanExecute
                .BindTo(this, x => x.Enabled);
            this.Events().MouseClick
                .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
                .Select(x => Unit.Default)
                .InvokeCommand(viewModel, x => x.DeleteGraphCommand);
        }
    }
}
