using NStack;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.ViewModel.Interface;
using Pathfinding.Domain.Core;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.View
{
    internal sealed partial class GraphNeighborhoodView : FrameView
    {
        private readonly CompositeDisposable disposables = [];

        public GraphNeighborhoodView(IRequireNeighborhoodNameViewModel viewModel)
        {
            var neighborhoods = Enum.GetValues<Neighborhoods>()
                .ToDictionary(x => x.ToStringRepresentation());
            Initialize();
            var labels = neighborhoods.Keys.Select(ustring.Make).ToArray();
            var values = labels.Select(x => neighborhoods[x.ToString()]).ToList();
            this.neighborhoods.RadioLabels = labels;
            this.neighborhoods.Events().SelectedItemChanged
                .Where(x => x.SelectedItem > -1)
                .Select(x => values[x.SelectedItem])
                .BindTo(viewModel, x => x.Neighborhood)
                .DisposeWith(disposables);
            this.neighborhoods.SelectedItem = 0;
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
