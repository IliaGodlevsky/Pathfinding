using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Domain.Core.Enums;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class RunsListView : FrameView
    {
        public RunsListView([KeyFilter(KeyFilters.Views)] IMessenger messenger,
            IRequireRunNameViewModel viewModel)
        {
            Initialize();
            var algos = Enum.GetValues<Algorithms>()
                .OrderBy(x => x.GetOrder())
                .ToArray();
            var source = algos
                .Select(x => x.ToStringRepresentation())
                .ToArray();
            this.Events().VisibleChanged
                .Where(x => Visible)
                .Do(x => runList.SelectedItem = runList.SelectedItem)
                .Subscribe();
            runList.SetSource(source);
            runList.Events().SelectedItemChanged
                .Where(x => x.Item > -1)
                .Select(x => algos[x.Item])
                .Do(algorithm =>
                {
                    // Don't change order of the messages
                    switch (algorithm)
                    {
                        case Algorithms.AStar:
                        case Algorithms.BidirectAStar:
                        case Algorithms.AStarGreedy:
                            messenger.Send(new OpenStepRuleViewMessage());
                            messenger.Send(new OpenRunsPopulateViewMessage());
                            messenger.Send(new OpenHeuristicsViewMessage());
                            break;
                        case Algorithms.Dijkstra:
                        case Algorithms.BidirectDijkstra:
                        case Algorithms.CostGreedy:
                            messenger.Send(new OpenStepRuleViewMessage());
                            messenger.Send(new CloseRunPopulateViewMessage());
                            messenger.Send(new CloseHeuristicsViewMessage());
                            break;
                        case Algorithms.Lee:
                        case Algorithms.BidirectLee:
                        case Algorithms.DepthFirst:
                        case Algorithms.Snake:
                        case Algorithms.Random:
                        case Algorithms.BidirectRandom:
                            messenger.Send(new CloseStepRulesViewMessage());
                            messenger.Send(new CloseRunPopulateViewMessage());
                            messenger.Send(new CloseHeuristicsViewMessage());
                            break;
                        case Algorithms.DistanceFirst:
                        case Algorithms.AStarLee:
                            messenger.Send(new CloseStepRulesViewMessage());
                            messenger.Send(new CloseRunPopulateViewMessage());
                            messenger.Send(new OpenHeuristicsViewMessage());
                            break;
                    }
                })
                .BindTo(viewModel, x => x.Algorithm);
        }
    }
}
