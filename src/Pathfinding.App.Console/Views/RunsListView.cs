using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Shared.Extensions;
using ReactiveMarbles.ObservableEvents;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunsListView : FrameView
{
    private readonly CompositeDisposable disposables = [];
    private readonly List<CheckBox> algorithms = [];
    private readonly IMessenger messenger;
    private readonly IList<Algorithms> selectedAlgorithms;
    private readonly Dictionary<Algorithms, (HashSet<Algorithms> Group, Action Action)> requirementGroups;

    private HashSet<Algorithms> RequirementGroup { get; set; } = [];

    public RunsListView(IMessenger messenger,
        IRunCreateViewModel viewModel)
    {
        Initialize();
        this.messenger = messenger;
        viewModel.SelectedAlgorithms.Clear();
        requirementGroups = CreateRequirementsGroup();
        selectedAlgorithms = viewModel.SelectedAlgorithms;
        foreach (var algorithm in viewModel.AllowedAlgorithms)
        {
            var text = algorithm.ToStringRepresentation();
            var checkBox = new CheckBox(text) { Y = algorithms.Count };
            checkBox.Events().Toggled.Do(toggled =>
            {
                if (!toggled && RequirementGroup.Count > 0)
                {
                    if (!RequirementGroup.Contains(algorithm))
                    {
                        checkBox.Checked = false;
                    }
                    else
                    {
                        viewModel.SelectedAlgorithms.Add(algorithm);
                    }
                }
                else if (toggled && RequirementGroup.Count > 0)
                {
                    viewModel.SelectedAlgorithms.Remove(algorithm);
                    if (viewModel.SelectedAlgorithms.Count == 0)
                    {
                        RequirementGroup = [];
                        HideAllParametresViews();
                    }
                    else
                    {
                        var algo = viewModel.SelectedAlgorithms[0];
                        RequirementGroup = requirementGroups[algo].Group;
                    }
                }
                else if (!toggled && RequirementGroup.Count == 0)
                {
                    RequirementGroup = requirementGroups[algorithm].Group;
                    viewModel.SelectedAlgorithms.Add(algorithm);
                    requirementGroups[algorithm].Action();
                }
            })  .Subscribe()
                .DisposeWith(disposables);
            algorithms.Add(checkBox);
        }
        Add([.. algorithms]);
    }

    private Dictionary<Algorithms, (HashSet<Algorithms>, Action)> CreateRequirementsGroup()
    {
        var result = new Dictionary<Algorithms, (HashSet<Algorithms>, Action)>();
        result.AddRange(CreateRequirementGroup(
            () =>
            {
                messenger.Send(new OpenStepRuleViewMessage());
                messenger.Send(new OpenRunsPopulateViewMessage());
                messenger.Send(new OpenHeuristicsViewMessage());
            },
            Algorithms.AStar,
            Algorithms.IdaStar,
            Algorithms.BidirectAStar,
            Algorithms.AStarGreedy));
        result.AddRange(CreateRequirementGroup(
            () =>
            {
                messenger.Send(new OpenStepRuleViewMessage());
                messenger.Send(new CloseRunPopulateViewMessage());
                messenger.Send(new CloseHeuristicsViewMessage());
            },
            Algorithms.Dijkstra,
            Algorithms.BidirectDijkstra,
            Algorithms.CostGreedy));
        result.AddRange(CreateRequirementGroup(
            () =>
            {
                messenger.Send(new CloseStepRulesViewMessage());
                messenger.Send(new CloseRunPopulateViewMessage());
                messenger.Send(new OpenHeuristicsViewMessage());
            },
            Algorithms.DistanceFirst,
            Algorithms.AStarLee));
        result.AddRange(CreateRequirementGroup(
            HideAllParametresViews,
            Algorithms.Lee,
            Algorithms.BidirectLee,
            Algorithms.DepthFirst,
            Algorithms.Snake,
            Algorithms.Random,
            Algorithms.BidirectRandom,
            Algorithms.DepthFirstRandom
            ));
        return result;
    }

    private static Dictionary<Algorithms, (HashSet<Algorithms>, Action)> CreateRequirementGroup(Action action, params Algorithms[] algorithms)
    {
        return algorithms.ToDictionary(x => x, _ => (new HashSet<Algorithms>(algorithms), action));
    }

    private void HideAllParametresViews()
    {
        messenger.Send(new CloseStepRulesViewMessage());
        messenger.Send(new CloseRunPopulateViewMessage());
        messenger.Send(new CloseHeuristicsViewMessage());
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}
