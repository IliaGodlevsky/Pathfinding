using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.Domain.Enums;
using Pathfinding.Presentation.Console.Extensions;
using Pathfinding.Presentation.Console.Messages.View;
using Pathfinding.Presentation.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class RunHeuristicsView
{
    private readonly IRequireHeuristicsViewModel viewModel;
    private readonly List<CheckBox> checkBoxes = [];

    private readonly CompositeDisposable disposables = [];

    public RunHeuristicsView(IMessenger messenger, IRequireHeuristicsViewModel viewModel)
    {
        Initialize();
        this.viewModel = viewModel;
        messenger.RegisterHandler<OpenHeuristicsViewMessage>(this, OnHeuristicsViewOpen).DisposeWith(disposables);
        messenger.RegisterHandler<CloseHeuristicsViewMessage>(this, OnHeuristicsViewClosed).DisposeWith(disposables);
        var heuristics = viewModel.AppliedHeuristics;
        heuristics.Clear();
        foreach (var heuristic in viewModel.AvailableHeuristics)
        {
            var text = heuristic.ToStringRepresentation();
            var checkBox = new CheckBox(text) { Y = checkBoxes.Count };
            checkBox.Events().Toggled
                .Select(x => x
                    ? (Action<Heuristics>)(z => heuristics.Remove(z))
                    : heuristics.Add)
                .Subscribe(x => x(heuristic))
                .DisposeWith(disposables);
            checkBoxes.Add(checkBox);
        }
        Add([.. checkBoxes]);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }

    private void OnHeuristicsViewOpen(OpenHeuristicsViewMessage msg)
    {
        Close();
        Visible = true;
    }

    private void OnHeuristicsViewClosed(CloseHeuristicsViewMessage msg)
    {
        Close();
        Visible = false;
    }

    private void Close()
    {
        foreach (var checkBox in checkBoxes.Where(x => x.Checked))
        {
            checkBox.Checked = false;
        }
        viewModel.AppliedHeuristics.Clear();
    }
}
