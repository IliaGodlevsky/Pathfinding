using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.ViewModels;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed class RunCreateDialog : Dialog
{
    private readonly CompositeDisposable disposables = [];

    public RunCreateDialog(IMessenger messenger, RunCreateViewModel viewModel)
    {
        var runListView = new RunsListView(messenger, viewModel).DisposeWith(disposables);
        var stepRulesView = new RunStepRulesView(messenger, viewModel).DisposeWith(disposables);
        var heuristicView = new RunHeuristicsView(messenger, viewModel).DisposeWith(disposables);
        var populateView = new RunsPopulateView(messenger, viewModel).DisposeWith(disposables);
        var runParametresView = new RunParametresView([stepRulesView,
            heuristicView, populateView]).DisposeWith(disposables);
        var createButton = new Button("Create").DisposeWith(disposables);
        var cancelButton = new Button("Cancel").DisposeWith(disposables);
        viewModel.CreateRunCommand.CanExecute
           .BindTo(createButton, x => x.Enabled)
           .DisposeWith(disposables);
        createButton.Events().MouseClick
           .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
           .Select(_ => Unit.Default)
           .Do(x => Application.RequestStop())
           .InvokeCommand(viewModel, x => x.CreateRunCommand)
           .DisposeWith(disposables);
        cancelButton.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Subscribe(_ => Application.RequestStop())
            .DisposeWith(disposables);
        Width = Dim.Percent(17);
        Height = Dim.Percent(43);
        Add(runListView, runParametresView);
        AddButton(cancelButton);
        AddButton(createButton);
        Title = "Create run";
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}
