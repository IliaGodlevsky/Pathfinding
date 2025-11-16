using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

using static Terminal.Gui.MouseFlags;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunProgressView : FrameView
{
    private static readonly InclusiveValueRange<float> FractionRange = (1, 0);

    private readonly IRunFieldViewModel viewModel;
    private readonly CompositeDisposable disposables = [];

    private float Fraction
    {
        get => bar.Fraction;
        set
        {
            bar.Fraction = FractionRange.ReturnInRange(value);
            viewModel.SelectedRun.Fraction = bar.Fraction;
        }
    }

    public RunProgressView(
        [KeyFilter(KeyFilters.Views)] IMessenger messenger,
        IRunFieldViewModel viewModel)
    {
        Initialize();
        messenger.RegisterHandler<CloseRunFieldMessage>(this, OnRunFieldClosed).DisposeWith(disposables);
        messenger.RegisterHandler<OpenRunFieldMessage>(this, OnRunFieldOpen).DisposeWith(disposables);
        messenger.RegisterHandler<KeyPressedMessage>(this, Clicked).DisposeWith(disposables);
        this.viewModel = viewModel;

        BindTo(leftLabel, _ => Fraction - GetFractionPerClick(), Button1Pressed, WheeledDown);
        BindTo(leftLabel, _ => Fraction - GetExtraFractionPerClick(), Button1Pressed | ButtonCtrl);
        BindTo(leftLabel, _ => Fraction + GetFractionPerClick(), WheeledUp);
        BindTo(leftLabel, _ => FractionRange.LowerValueOfRange, Button2Clicked);
        BindTo(rightLabel, _ => Fraction + GetFractionPerClick(), Button1Pressed, WheeledUp);
        BindTo(rightLabel, _ => Fraction + GetExtraFractionPerClick(), Button1Pressed | ButtonCtrl);
        BindTo(rightLabel, _ => Fraction - GetFractionPerClick(), WheeledDown);
        BindTo(rightLabel, _ => FractionRange.UpperValueOfRange, Button2Clicked);
        BindTo(bar, x => (float)Math.Round((x.MouseEvent.X + 1f) / bar.Bounds.Width, 3), Button1Clicked);
        viewModel.WhenAnyValue(x => x.SelectedRun.Fraction)
            .BindTo(this, x => x.Fraction)
            .DisposeWith(disposables);
    }

    private void Clicked(KeyPressedMessage msg)
    {
        var key = msg.Args.KeyEvent.Key;
        switch (key)
        {
            case Key.CursorLeft | Key.CtrlMask:
            case Key.D1:
                Fraction = FractionRange.LowerValueOfRange;
                msg.Args.Handled = true;
                break;
            case Key.CursorRight | Key.CtrlMask:
            case Key.D0:
                Fraction = FractionRange.UpperValueOfRange;
                msg.Args.Handled = true;
                break;
            case Key.CursorLeft | Key.ShiftMask:
                Fraction -= GetExtraFractionPerClick();
                msg.Args.Handled = true;
                break;
            case Key.CursorRight | Key.ShiftMask:
                Fraction += GetExtraFractionPerClick();
                msg.Args.Handled = true;
                break;
            case Key.CursorLeft:
                Fraction -= GetFractionPerClick();
                msg.Args.Handled = true;
                break;
            case Key.CursorRight:
                Fraction += GetFractionPerClick();
                msg.Args.Handled = true;
                break;
            case Key.D2:
            case Key.D3:
            case Key.D4:
            case Key.D5:
            case Key.D6:
            case Key.D7:
            case Key.D8:
            case Key.D9:
                Fraction = (float)Math.Round((float)((int)key - (int)Key.D1) / 9, 3);
                msg.Args.Handled = true;
                break;
        }
    }

    private static float GetFractionPerClick()
    {
        return Settings.Default.FractionPerClick;
    }

    private static float GetExtraFractionPerClick()
    {
        return GetFractionPerClick() * 3;
    }

    private void BindTo(View view, Func<MouseEventArgs, float> function,
        params MouseFlags[] flags)
    {
        view.Events().MouseClick
            .Where(x => viewModel.SelectedRun != RunModel.Empty
                && flags.Any(z => x.MouseEvent.Flags.HasFlag(z)))
            .Select(function)
            .BindTo(this, x => x.Fraction)
            .DisposeWith(disposables);
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }

    private void OnRunFieldClosed(CloseRunFieldMessage msg)
    {
        rightLabel.Visible = false;
        leftLabel.Visible = false;
        bar.Visible = false;
    }

    private void OnRunFieldOpen(OpenRunFieldMessage msg)
    {
        rightLabel.Visible = true;
        leftLabel.Visible = true;
        bar.Visible = true;
    }
}
