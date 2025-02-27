using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class GraphAssembleButton : Button
    {
        public GraphAssembleButton(GraphAssembleView view)
        {
            Initialize();
            this.Events().MouseClick
                .Select(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
                .BindTo(view, x => x.Visible);
        }
    }
}
