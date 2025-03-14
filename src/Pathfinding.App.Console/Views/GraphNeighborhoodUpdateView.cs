using DynamicData;
using NStack;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Domain.Core.Enums;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class GraphNeighborhoodUpdateView : FrameView
    {
        public GraphNeighborhoodUpdateView(GraphUpdateViewModel viewModel)
        {
            Initialize();
            var factories = Enum.GetValues<Neighborhoods>()
                .ToDictionary(x => x.ToStringRepresentation());
            var radioLabels = factories.Keys
                .Select(ustring.Make)
                .ToArray();
            var values = radioLabels
                .Select(x => factories[x.ToString()])
                .ToArray();
            neighborhoods.RadioLabels = radioLabels;
            neighborhoods.Events().SelectedItemChanged
                .Where(x => x.SelectedItem > -1)
                .Select(x => values[x.SelectedItem])
                .BindTo(viewModel, x => x.Neighborhood);
            viewModel.WhenAnyValue(x => x.Neighborhood)
                .Select(x => factories.Values.IndexOf(x))
                .BindTo(neighborhoods, x => x.SelectedItem);
        }
    }
}
