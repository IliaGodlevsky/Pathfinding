using NStack;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Domain.Core.Enums;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class GraphSmoothLevelView : FrameView
    {
        public GraphSmoothLevelView(IRequireSmoothLevelViewModel viewModel)
        {
            var smoothLevels = Enum.GetValues<SmoothLevels>()
                .ToDictionary(x => x.ToStringRepresentation());
            Initialize();
            var labels = smoothLevels.Keys.Select(ustring.Make).ToArray();
            var values = labels.Select(x => smoothLevels[x.ToString()]).ToList();
            this.smoothLevels.RadioLabels = labels;
            this.smoothLevels.Events()
                .SelectedItemChanged
                .Where(x => x.SelectedItem > -1)
                .Select(x => values[x.SelectedItem])
                .BindTo(viewModel, x => x.SmoothLevel);
            this.smoothLevels.SelectedItem = 0;
            VisibleChanged += OnVisibilityChanged;
        }

        private void OnVisibilityChanged()
        {
            if (Visible)
            {
                smoothLevels.SelectedItem = 0;
            }
        }
    }
}
