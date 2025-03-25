using Pathfinding.App.Console.Models;
using Pathfinding.Domain.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunCreateButton : Button
{
    public RunCreateButton(RunCreateView view,
        IPathfindingRange<GraphVertexModel> pathfindingRange)
    {
        Initialize();

        this.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Do(x => view.Visible = true)
            .Subscribe();
        pathfindingRange.WhenAnyValue(x => x.Source, x => x.Target,
            (source, target) => source is not null && target is not null)
            .BindTo(this, x => x.Enabled);
    }
}
