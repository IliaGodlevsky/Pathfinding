using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class RunHeuristicsView : FrameView
    {
        private readonly IRequireHeuristicsViewModel viewModel;
        private readonly List<CheckBox> checkBoxes = [];

        public RunHeuristicsView([KeyFilter(KeyFilters.Views)] IMessenger messenger,
            IRequireHeuristicsViewModel viewModel)
        {
            Initialize();
            int i = 0;
            foreach (var function in viewModel.AllowedHeuristics)
            {
                var text = function.ToStringRepresentation();
                var checkBox = new CheckBox(text) { Y = i++ };
                checkBox.Events().Toggled
                    .Do(x =>
                    {
                        if (x) { viewModel.Heuristics.Remove(function); }
                        else { viewModel.Heuristics.Add(function); }
                    })
                    .Subscribe();
                Add(checkBox);
                checkBoxes.Add(checkBox);
            }
            messenger.Register<OpenHeuristicsViewMessage>(this, OnHeuristicsViewOpen);
            messenger.Register<CloseHeuristicsViewMessage>(this, OnHeuristicsViewClosed);
            messenger.Register<CloseRunCreateViewMessage>(this, OnRunCreationViewClosed);
            this.viewModel = viewModel;
        }

        private void OnHeuristicsViewOpen(object recipient, OpenHeuristicsViewMessage msg)
        {
            Close();
            viewModel.Heuristics.Add(default);
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
            viewModel.Heuristics.Clear();
            Visible = false;
        }
    }
}
