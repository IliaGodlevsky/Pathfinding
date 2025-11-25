using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using NStack;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunStepRulesView : FrameView
{
    private readonly IRequireStepRuleViewModel viewModel;
    private readonly CompositeDisposable disposables = [];

    public RunStepRulesView(
        [KeyFilter(KeyFilters.Views)] IMessenger messenger,
        IRequireStepRuleViewModel viewModel)
    {
        Initialize();
        var rules = viewModel.AllowedStepRules
            .ToDictionary(x => x.ToStringRepresentation());
        var labels = rules.Select(x => ustring.Make(x.Key)).ToArray();
        var values = labels.Select(x => rules[x.ToString()!]).ToList();
        stepRules.RadioLabels = labels;
        stepRules.Events().SelectedItemChanged
            .Where(x => x.SelectedItem > -1)
            .Select(x => values[x.SelectedItem])
            .BindTo(viewModel, x => x.StepRule)
            .DisposeWith(disposables);
        stepRules.SelectedItem = 0;
        messenger.RegisterHandler<OpenStepRuleViewMessage>(this, OnOpen).DisposeWith(disposables);
        messenger.RegisterHandler<CloseStepRulesViewMessage>(this, OnStepRulesViewClose).DisposeWith(disposables);
        this.viewModel = viewModel;
    }

    private void OnOpen(OpenStepRuleViewMessage msg)
    {
        stepRules.SelectedItem = 0;
        Visible = true;
    }

    private void OnStepRulesViewClose(CloseStepRulesViewMessage msg)
    {
        Close();
    }

    private void Close()
    {
        viewModel.StepRule = default;
        Visible = false;
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}
