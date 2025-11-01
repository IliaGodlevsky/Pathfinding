using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Domain.Core.Enums;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunsListView : FrameView
{
    private readonly CompositeDisposable disposables = [];

    public RunsListView([KeyFilter(KeyFilters.Views)] IMessenger messenger,
        IRunCreateViewModel viewModel)
    {
        Initialize();
        var source = viewModel.AllowedAlgorithms
            .ToDictionary(x => (object)x.ToStringRepresentation());
        this.Events().VisibleChanged
            .Where(_ => Visible)
            .Do(x => runList.SelectedItem = runList.SelectedItem)
            .Subscribe()
            .DisposeWith(disposables);
        runList.SetSource(source.Keys.ToList());
        runList.Events().SelectedItemChanged
            .Where(x => x.Item > -1)
            .Select(x => source[x.Value])
            .Do(algorithm =>
            {
                // Don't change order of the messages
                switch (algorithm)
                {
                    case Algorithms.AStar:
                    case Algorithms.IdaStar:
                    case Algorithms.BidirectAStar:
                    case Algorithms.AStarGreedy:
                        messenger.Send(new OpenStepRuleViewMessage());
                        messenger.Send(new OpenRunsPopulateViewMessage());
                        messenger.Send(new OpenHeuristicsViewMessage());
                        messenger.Send(new CloseBeamWidthViewMessage());
                        break;
                    case Algorithms.Dijkstra:
                    case Algorithms.BidirectDijkstra:
                    case Algorithms.CostGreedy:
                        messenger.Send(new OpenStepRuleViewMessage());
                        messenger.Send(new CloseRunPopulateViewMessage());
                        messenger.Send(new CloseHeuristicsViewMessage());
                        messenger.Send(new CloseBeamWidthViewMessage());
                        break;
                    case Algorithms.DistanceFirst:
                    case Algorithms.AStarLee:
                        messenger.Send(new CloseStepRulesViewMessage());
                        messenger.Send(new CloseRunPopulateViewMessage());
                        messenger.Send(new OpenHeuristicsViewMessage());
                        messenger.Send(new CloseBeamWidthViewMessage());
                        break;
                    case Algorithms.BeamSearch:
                        messenger.Send(new CloseStepRulesViewMessage());
                        messenger.Send(new CloseRunPopulateViewMessage());
                        messenger.Send(new OpenHeuristicsViewMessage());
                        messenger.Send(new OpenBeamWidthViewMessage());
                        break;
                    default:
                        messenger.Send(new CloseStepRulesViewMessage());
                        messenger.Send(new CloseRunPopulateViewMessage());
                        messenger.Send(new CloseHeuristicsViewMessage());
                        messenger.Send(new CloseBeamWidthViewMessage());
                        break;
                }
            })
            .BindTo(viewModel, x => x.Algorithm)
            .DisposeWith(disposables);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}
