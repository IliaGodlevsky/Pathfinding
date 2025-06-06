﻿using Pathfinding.App.Console.Resources;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed class GraphCopyButton : Button
{
    public GraphCopyButton(IGraphCopyViewModel viewModel)
    {
        Text = Resource.Copy;
        viewModel.CopyGraphCommand.CanExecute
            .BindTo(this, x => x.Enabled);
        this.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Select(x => Unit.Default)
            .InvokeCommand(viewModel, x => x.CopyGraphCommand);
    }
}
