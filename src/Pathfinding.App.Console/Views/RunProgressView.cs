using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunProgressView : FrameView
{
    private const float FractionPerClick = 0.015f;
    private const float ExtraFractionPerClick = FractionPerClick * 3;

    private readonly CompositeDisposable disposables = [];

    public RunProgressView(
        [KeyFilter(KeyFilters.Views)] IMessenger messenger,
        IRunFieldViewModel viewModel)
    {
        Initialize();
        messenger.Register<CloseRunFieldMessage>(this, OnRunFieldClosed);
        messenger.Register<OpenRunFieldMessage>(this, OnRunFieldOpen);
        var leftLabelMouseEvent = leftLabel.Events().MouseClick
            .Where(x => viewModel.SelectedRun != RunModel.Empty);
        var rightLabelMouseEvent = rightLabel.Events().MouseClick
            .Where(x => viewModel.SelectedRun != RunModel.Empty);
        leftLabelMouseEvent
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Pressed)
            .Select(x => bar.Fraction - FractionPerClick)
            .BindTo(viewModel, x => x.SelectedRun.Fraction)
            .DisposeWith(disposables);
        leftLabelMouseEvent
            .Where(x => x.MouseEvent.Flags.HasFlag(MouseFlags.Button1Pressed)
                && x.MouseEvent.Flags.HasFlag(MouseFlags.ButtonCtrl))
            .Select(x => bar.Fraction - ExtraFractionPerClick)
            .BindTo(viewModel, x => x.SelectedRun.Fraction)
            .DisposeWith(disposables);
        leftLabelMouseEvent
            .Where(x => x.MouseEvent.Flags == MouseFlags.WheeledDown)
            .Select(x => bar.Fraction - FractionPerClick)
            .BindTo(viewModel, x => x.SelectedRun.Fraction)
            .DisposeWith(disposables);
        leftLabelMouseEvent
            .Where(x => x.MouseEvent.Flags == MouseFlags.WheeledUp)
            .Select(x => bar.Fraction + FractionPerClick)
            .BindTo(viewModel, x => x.SelectedRun.Fraction)
            .DisposeWith(disposables);
        leftLabelMouseEvent
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button2Clicked)
            .Select(x => RunModel.FractionRange.LowerValueOfRange)
            .BindTo(viewModel, x => x.SelectedRun.Fraction)
            .DisposeWith(disposables);
        rightLabelMouseEvent
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Pressed)
            .Select(x => bar.Fraction + FractionPerClick)
            .BindTo(viewModel, x => x.SelectedRun.Fraction)
            .DisposeWith(disposables);
        rightLabelMouseEvent
            .Where(x => x.MouseEvent.Flags.HasFlag(MouseFlags.Button1Pressed)
                && x.MouseEvent.Flags.HasFlag(MouseFlags.ButtonCtrl))
            .Select(x => bar.Fraction + ExtraFractionPerClick)
            .BindTo(viewModel, x => x.SelectedRun.Fraction)
            .DisposeWith(disposables);
        rightLabelMouseEvent
            .Where(x => x.MouseEvent.Flags == MouseFlags.WheeledDown)
            .Select(x => bar.Fraction - FractionPerClick)
            .BindTo(viewModel, x => x.SelectedRun.Fraction)
            .DisposeWith(disposables);
        rightLabelMouseEvent
            .Where(x => x.MouseEvent.Flags == MouseFlags.WheeledUp)
            .Select(x => bar.Fraction + FractionPerClick)
            .BindTo(viewModel, x => x.SelectedRun.Fraction)
            .DisposeWith(disposables);
        rightLabelMouseEvent
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button2Clicked)
            .Select(x => RunModel.FractionRange.UpperValueOfRange)
            .BindTo(viewModel, x => x.SelectedRun.Fraction)
            .DisposeWith(disposables);
        bar.Events().MouseClick
            .Where(x => viewModel.SelectedRun != RunModel.Empty)
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Select(x => (float)Math.Round((float)(x.MouseEvent.X + 1) / bar.Bounds.Width, 3))
            .BindTo(viewModel, x => x.SelectedRun.Fraction)
            .DisposeWith(disposables);
        viewModel.WhenAnyValue(x => x.SelectedRun.Fraction)
            .BindTo(bar, x => x.Fraction)
            .DisposeWith(disposables);
    }

    private void OnRunFieldClosed(object recipient, CloseRunFieldMessage msg)
    {
        rightLabel.Visible = false;
        leftLabel.Visible = false;
        bar.Visible = false;
    }

    private void OnRunFieldOpen(object recipient, OpenRunFieldMessage msg)
    {
        rightLabel.Visible = true;
        leftLabel.Visible = true;
        bar.Visible = true;
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }
}
