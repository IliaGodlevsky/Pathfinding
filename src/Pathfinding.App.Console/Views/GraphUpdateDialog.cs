using Pathfinding.App.Console.ViewModels;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed class GraphUpdateDialog : Dialog
{
    private readonly CompositeDisposable disposables = [];

    public GraphUpdateDialog(GraphUpdateViewModel viewModel)
    {
        var nameField = new GraphNameUpdateView(viewModel).DisposeWith(disposables);
        var neighborhood = new GraphNeighborhoodUpdateView(viewModel).DisposeWith(disposables);
        var updateButton = new Button("Update").DisposeWith(disposables);
        var cancelButton = new Button("Cancel").DisposeWith(disposables);
        Width = Dim.Percent(18);
        Height = Dim.Percent(30);
        viewModel.UpdateGraphCommand.CanExecute
            .BindTo(updateButton, x => x.Enabled)
            .DisposeWith(disposables);
        updateButton.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Do(_ => Application.RequestStop())
            .Select(_ => Unit.Default)
            .InvokeCommand(viewModel, x => x.UpdateGraphCommand)
            .DisposeWith(disposables);
        cancelButton.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Subscribe(_ => Application.RequestStop())
            .DisposeWith(disposables);
        Add(nameField, neighborhood);
        AddButton(cancelButton);
        AddButton(updateButton);
        Title = "Update graph";
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}
