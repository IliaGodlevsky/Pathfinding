using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using NStack;
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

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class RunHeuristicsView : FrameView
    {
        private readonly ustring[] radioLabels;
        private readonly CompositeDisposable disposables = [];
        private readonly IRequireHeuristicsViewModel heuristicsViewModel;

        public RunHeuristicsView(
            [KeyFilter(KeyFilters.Views)] IMessenger messenger,
            IRequireHeuristicsViewModel heuristicsViewModel)
        {
            Initialize();
            var heurs = Enum.GetValues<HeuristicFunctions>()
                .ToDictionary(x => x.ToStringRepresentation());
            var labels = heurs.Keys.Select(ustring.Make).ToArray();
            var values = labels.Select(x => heurs[x.ToString()]).ToList();
            radioLabels = labels;
            heuristics.RadioLabels = radioLabels;
            heuristics.Events().SelectedItemChanged
               .Where(x => x.SelectedItem > -1)
               .Select(x => values[x.SelectedItem])
               .BindTo(heuristicsViewModel, x => x.Heuristic)
               .DisposeWith(disposables);
            messenger.Register<OpenHeuristicsViewMessage>(this, OnOpen);
            messenger.Register<CloseHeuristicsViewMessage>(this, OnHeuristicsViewClosed);
            messenger.Register<CloseRunCreateViewMessage>(this, OnRunCreationViewClosed);
            this.heuristicsViewModel = heuristicsViewModel;
        }

        private void OnOpen(object recipient, OpenHeuristicsViewMessage msg)
        {
            heuristics.SelectedItem = 0;

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
            heuristicsViewModel.FromWeight = null;
            heuristicsViewModel.Heuristic = null;
            heuristicsViewModel.ToWeight = null;
            heuristicsViewModel.Step = null;
            Visible = false;
        }
    }
}
