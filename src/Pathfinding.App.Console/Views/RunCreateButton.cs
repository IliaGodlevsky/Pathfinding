using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels;
using Pathfinding.Domain.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunCreateButton : Button
{
    private readonly CompositeDisposable disposables = [];

    public RunCreateButton(
        [KeyFilter(KeyFilters.Views)] IMessenger messenger,
        RunCreateViewModel viewModel,
        IPathfindingRange<GraphVertexModel> pathfindingRange)
    {
        Initialize();
        this.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Subscribe(_ => ShowDialog(messenger, viewModel))
            .DisposeWith(disposables);
        pathfindingRange.WhenAnyValue(x => x.Source, x => x.Target,
            (source, target) => source is not null && target is not null)
            .BindTo(this, x => x.Enabled)
            .DisposeWith(disposables);
    }

    private static void ShowDialog(IMessenger messenger,
        RunCreateViewModel viewModel)
    {
        var dialog = new RunCreateDialog(messenger, viewModel);
        Application.Run(dialog);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}
