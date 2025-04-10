﻿using NStack;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Domain.Core.Enums;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphSmoothLevelUpdateView : FrameView
{
    public GraphSmoothLevelUpdateView(GraphUpdateViewModel viewModel)
    {
        var lvls = Enum.GetValues<SmoothLevels>()
            .ToDictionary(x => x.ToStringRepresentation());
        Initialize();
        var labels = lvls.Keys.Select(ustring.Make).ToArray();
        var values = labels.Select(x => lvls[x.ToString()]).ToList();
        smoothLevels.RadioLabels = labels;
        smoothLevels.Events()
            .SelectedItemChanged
            .Where(x => x.SelectedItem > -1)
            .Select(x => values[x.SelectedItem])
            .BindTo(viewModel, x => x.SmoothLevel);
        viewModel.WhenAnyValue(x => x.SmoothLevel)
            .Select(x => values.IndexOf(x))
            .BindTo(smoothLevels, x => x.SelectedItem);
    }
}
