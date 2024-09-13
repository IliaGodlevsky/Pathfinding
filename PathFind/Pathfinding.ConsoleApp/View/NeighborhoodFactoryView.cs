﻿using NStack;
using Pathfinding.ConsoleApp.ViewModel;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.ConsoleApp.View
{
    internal sealed partial class NeighborhoodFactoryView : FrameView
    {
        private readonly CompositeDisposable disposables = new();
        private readonly NeighborhoodFactoryViewModel factoryViewModel;
        private readonly CreateGraphViewModel viewModel;

        public NeighborhoodFactoryView(
            NeighborhoodFactoryViewModel factoryViewModel,
            CreateGraphViewModel viewModel)
        {
            this.factoryViewModel = factoryViewModel;
            this.viewModel = viewModel;
            Initialize();
            var neighbors = factoryViewModel.Factories.Select(x => x.Value).ToList();
            neighborhoods.RadioLabels = factoryViewModel.Factories.Keys
                .Select(x => ustring.Make(x))
                .ToArray();
            neighborhoods.Events().SelectedItemChanged
                .Where(x => x.SelectedItem > -1)
                .Select(x => neighbors[x.SelectedItem])
                .BindTo(this.viewModel, x => x.NeighborhoodFactory)
                .DisposeWith(disposables);
            neighborhoods.SelectedItem = 0;
            VisibleChanged += OnVisibilityChanged;
        }

        private void OnVisibilityChanged()
        {
            if (Visible)
            {
                neighborhoods.SelectedItem = 0;
            }
        }
    }
}
