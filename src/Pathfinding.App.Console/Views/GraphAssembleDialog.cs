using Pathfinding.App.Console.ViewModels;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed class GraphAssembleDialog : Dialog
{
    private readonly CompositeDisposable disposables = [];

    public GraphAssembleDialog(GraphAssembleViewModel viewModel)
    {
        var name = new GraphNameView(viewModel).DisposeWith(disposables);
        var parametres = new GraphParametresView(viewModel).DisposeWith(disposables);
        var neighborhood = new GraphNeighborhoodView(viewModel).DisposeWith(disposables);
        var smoothLevels = new GraphSmoothLevelView(viewModel).DisposeWith(disposables);
        var createButton = new Button("Create").DisposeWith(disposables);
        var cancelButton = new Button("Cancel").DisposeWith(disposables);
        viewModel.AssembleGraphCommand.CanExecute
           .BindTo(createButton, x => x.Enabled)
           .DisposeWith(disposables);
        createButton.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Select(_ => Unit.Default)
            .Do(x => Application.RequestStop())
            .InvokeCommand(viewModel, x => x.AssembleGraphCommand)
            .DisposeWith(disposables);
        cancelButton.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Subscribe(_ => Application.RequestStop())
            .DisposeWith(disposables);
        Add(name, parametres, neighborhood, smoothLevels);
        Width = Dim.Percent(27);
        Height = Dim.Percent(33);
        AddButton(cancelButton);
        AddButton(createButton);
        Title = "Assemble graph";
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}
