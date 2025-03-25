using Autofac.Features.AttributeFilters;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphAssembleView : FrameView
{
    public GraphAssembleView(
        [KeyFilter(KeyFilters.GraphAssembleView)] View[] children,
        IGraphAssembleViewModel viewModel)
    {
        Initialize();
        Add(children);
        viewModel.AssembleGraphCommand.CanExecute
            .BindTo(createButton, x => x.Enabled);
        createButton.Events()
            .MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Select(x => Unit.Default)
            .Do(x => Visible = false)
            .InvokeCommand(viewModel, x => x.AssembleGraphCommand);
        foreach (var child in children)
        {
            this.Events().VisibleChanged
                .Select(x => Visible)
                .BindTo(child, x => x.Visible);
        }
        cancelButton.Events().MouseClick
            .Where(e => e.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Do(x =>
            {
                Visible = false;
                Application.Driver.SetCursorVisibility(CursorVisibility.Invisible);
            })
            .Subscribe();
    }
}
