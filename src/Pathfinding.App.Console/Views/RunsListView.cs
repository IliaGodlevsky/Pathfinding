using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunsListView : FrameView
{
    private readonly IMessenger messenger;
    private readonly CompositeDisposable disposables = [];

    private AlgorithmRequirements? Requirements { get; set; } = null;

    public RunsListView(IMessenger messenger, IRunCreateViewModel viewModel)
    {
        Initialize();
        this.messenger = messenger;
        var requirements = viewModel.Requirements;
        var selectedAlgorithms = viewModel.SelectedAlgorithms;
        selectedAlgorithms.Clear();
        int i = 0;
        foreach (var algorithm in viewModel.AllowedAlgorithms)
        {
            var text = algorithm.ToStringRepresentation();
            var checkBox = new CheckBox(text) { Y = i++ };
            checkBox.Events().Toggled.Subscribe(toggled =>
            {
                if (!toggled && Requirements != null)
                {
                    if (requirements[algorithm] != Requirements)
                    {
                        checkBox.Checked = false;
                    }
                    else
                    {
                        selectedAlgorithms.Add(algorithm);
                    }
                }
                else if (toggled && Requirements != null)
                {
                    selectedAlgorithms.Remove(algorithm);
                    if (selectedAlgorithms.Count == 0)
                    {
                        Requirements = null;
                        PerformRequirementAction(Requirements);
                    }
                }
                else if (!toggled && Requirements == null)
                {
                    Requirements = requirements[algorithm];
                    PerformRequirementAction(Requirements);
                    selectedAlgorithms.Add(algorithm);
                }
            }).DisposeWith(disposables);
            Add(checkBox);
        }
    }

    private void PerformRequirementAction(AlgorithmRequirements? requirements)
    {
        switch (requirements)
        {
            case AlgorithmRequirements.RequiresStepRule:
                messenger.Send(new OpenStepRuleViewMessage());
                messenger.Send(new CloseRunPopulateViewMessage());
                messenger.Send(new CloseHeuristicsViewMessage());
                break;
            case AlgorithmRequirements.RequiresHeuristics:
                messenger.Send(new CloseStepRulesViewMessage());
                messenger.Send(new CloseRunPopulateViewMessage());
                messenger.Send(new OpenHeuristicsViewMessage());
                break;
            case AlgorithmRequirements.RequiresAll:
                messenger.Send(new OpenStepRuleViewMessage());
                messenger.Send(new OpenRunsPopulateViewMessage());
                messenger.Send(new OpenHeuristicsViewMessage());
                break;
            default:
                messenger.Send(new CloseStepRulesViewMessage());
                messenger.Send(new CloseRunPopulateViewMessage());
                messenger.Send(new CloseHeuristicsViewMessage());
                break;
        }
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}
