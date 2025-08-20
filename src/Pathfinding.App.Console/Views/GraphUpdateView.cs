using Autofac.Features.AttributeFilters;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.ViewModels;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphUpdateView : FrameView
{
    private readonly CompositeDisposable disposables = [];

    public GraphUpdateView(
        [KeyFilter(KeyFilters.GraphUpdateView)] View[] children,
        GraphUpdateViewModel viewModel)
    {
        Initialize();
        Add(children);
        viewModel.UpdateGraphCommand.CanExecute
            .BindTo(updateButton, x => x.Enabled)
            .DisposeWith(disposables);
        updateButton.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Select(x => Unit.Default)
            .Do(x => Hide())
            .InvokeCommand(viewModel, x => x.UpdateGraphCommand)
            .DisposeWith(disposables);
        cancelButton.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Do(x => Hide())
            .Subscribe()
            .DisposeWith(disposables);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }

    private void Hide()
    {
        Visible = false;
        Application.Driver.SetCursorVisibility(CursorVisibility.Invisible);
    }
}
