using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Domain.Core.Enums;
using ReactiveMarbles.ObservableEvents;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunHeuristicsView : FrameView
{
    private readonly IRequireHeuristicsViewModel viewModel;
    private readonly List<CheckBox> checkBoxes = [];

    public RunHeuristicsView([KeyFilter(KeyFilters.Views)] IMessenger messenger,
        IRequireHeuristicsViewModel viewModel)
    {
        Initialize();
        var heuristics = viewModel.AppliedHeuristics;
        foreach (var heuristic in viewModel.AllowedHeuristics)
        {
            var text = heuristic.ToStringRepresentation();
            var checkBox = new CheckBox(text) { Y = checkBoxes.Count };
            checkBox.Events().Toggled
                .Select(x => x
                    ? (Action<Heuristics?>)(x => heuristics.Remove(x))
                    : heuristics.Add)
                .Do(x => x(heuristic))
                .Subscribe();
            checkBoxes.Add(checkBox);
        }
        Add([.. checkBoxes]);
        messenger.Register<OpenHeuristicsViewMessage>(this, OnHeuristicsViewOpen);
        messenger.Register<CloseHeuristicsViewMessage>(this, OnHeuristicsViewClosed);
        messenger.Register<CloseRunCreateViewMessage>(this, OnRunCreationViewClosed);
        this.viewModel = viewModel;
    }

    private void OnHeuristicsViewOpen(object recipient, OpenHeuristicsViewMessage msg)
    {
        Close();
        viewModel.AppliedHeuristics.Add(default);
        Visible = true;
    }

    private void OnHeuristicsViewClosed(object recipient, CloseHeuristicsViewMessage msg)
    {
        Close();
    }

    private void OnRunCreationViewClosed(object recipient, CloseRunCreateViewMessage msg)
    {
        Close();
    }

    private void Close()
    {
        foreach (var checkBox in checkBoxes.Where(x => x.Checked))
        {
            checkBox.Checked = false;
            checkBox.OnToggled(true);
        }
        viewModel.AppliedHeuristics.Clear();
        Visible = false;
    }
}
