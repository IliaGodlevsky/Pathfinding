using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Extensions;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;
using static Terminal.Gui.Key;
using static Terminal.Gui.MouseFlags;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunProgressView : FrameView
{
    private readonly IRunFieldViewModel viewModel;
    private readonly CompositeDisposable disposables = [];

    private float Fraction
    {
        get => bar.Fraction;
        set => bar.Fraction = value;
    }

    public RunProgressView(
        [KeyFilter(KeyFilters.Views)] IMessenger messenger,
        IRunFieldViewModel viewModel)
    {
        Initialize();
        messenger.RegisterHandler<CloseRunFieldMessage>(this, OnRunFieldClosed).DisposeWith(disposables);
        messenger.RegisterHandler<OpenRunFieldMessage>(this, OnRunFieldOpen).DisposeWith(disposables);
        messenger.RegisterHandler<KeyPressedMessage, int>(this, Tokens.ProgressBar,
            msg => bar.OnKeyDown(msg.Args.KeyEvent)).DisposeWith(disposables);
        this.viewModel = viewModel;

        BindTo(leftLabel, _ => RunModel.FractionRange.LowerValueOfRange, Button2Clicked);
        BindTo(leftLabel, _ => Fraction - GetFractionPerClick(), Button1Pressed, WheeledDown);
        BindTo(leftLabel, _ => Fraction - GetExtraFractionPerClick(), Button1Pressed | ButtonCtrl);
        BindTo(leftLabel, _ => Fraction + GetFractionPerClick(), WheeledUp);
        BindTo(rightLabel, _ => RunModel.FractionRange.UpperValueOfRange, Button2Clicked);
        BindTo(rightLabel, _ => Fraction + GetFractionPerClick(), Button1Pressed, WheeledUp);
        BindTo(rightLabel, _ => Fraction + GetExtraFractionPerClick(), Button1Pressed | ButtonCtrl);
        BindTo(rightLabel, _ => Fraction - GetFractionPerClick(), WheeledDown);
        BindTo(bar, x => (float)Math.Round((x.MouseEvent.X + 1f) / bar.Bounds.Width, 3), Button1Clicked);
        BindTo(bar, x => (float)Math.Round(((int)x.KeyEvent.Key - (int)D1) / 9f, 3), D2, D3, D4, D5, D6, D7, D8, D9);
        BindTo(bar, _ => Fraction - GetFractionPerClick(), CursorLeft);
        BindTo(bar, _ => Fraction + GetFractionPerClick(), CursorRight);
        BindTo(bar, _ => Fraction - GetExtraFractionPerClick(), CursorLeft | ShiftMask);
        BindTo(bar, _ => Fraction + GetExtraFractionPerClick(), CursorRight | ShiftMask);
        BindTo(bar, _ => RunModel.FractionRange.LowerValueOfRange, CursorLeft | CtrlMask, D1);
        BindTo(bar, _ => RunModel.FractionRange.UpperValueOfRange, CursorRight | CtrlMask, D0);
        BindTo(bar, _ => Fraction > RunModel.FractionRange.LowerValueOfRange
                ? RunModel.FractionRange.LowerValueOfRange 
                : RunModel.FractionRange.UpperValueOfRange, Space);
        viewModel.WhenAnyValue(x => x.SelectedRun.Fraction)
            .BindTo(this, x => x.Fraction)
            .DisposeWith(disposables);
    }

    private static float GetFractionPerClick()
    {
        return Settings.Default.FractionPerClick;
    }

    private static float GetExtraFractionPerClick()
    {
        return GetFractionPerClick() * 3;
    }

    private void BindTo(View view, Func<MouseEventArgs, float> function, params MouseFlags[] flags)
    {
        BindTo(() => view.Events().MouseClick, function, (x, y) => y.MouseEvent.Flags == x, flags);
    }

    private void BindTo(View view, Func<KeyEventEventArgs, float> function, params Key[] flags)
    {
        BindTo(() => view.Events().KeyDown, function, (x, y) => y.KeyEvent.Key == x, flags);
    }

    private void BindTo<TArgs, TEnum>(Func<IObservable<TArgs>> trackedEvent,
        Func<TArgs, float> function, Func<TEnum, TArgs, bool> predicate, params TEnum[] flags)
    {
        trackedEvent()
            .Where(x => viewModel.SelectedRun != RunModel.Empty
                && flags.Any(z => predicate(z, x)))
            .Select(function)
            .BindTo(viewModel, x => x.SelectedRun.Fraction)
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
