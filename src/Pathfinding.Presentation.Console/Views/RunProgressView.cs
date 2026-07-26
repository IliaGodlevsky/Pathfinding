using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.Presentation.Console.Extensions;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Messages.View;
using Pathfinding.Presentation.Console.Models;
using Pathfinding.Presentation.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;
using static Terminal.Gui.Key;
using static Terminal.Gui.MouseFlags;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class RunProgressView : FrameView
{
    private readonly IRunFieldViewModel viewModel;
    private readonly CompositeDisposable disposables = [];
    private readonly SerialDisposable playback = new();
    private static readonly double[] PlaybackSpeeds = [0.25, 0.5, 1, 2, 4];
    private int playbackSpeedIndex = 2;

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
        this.viewModel = viewModel;

        messenger.RegisterHandler<CloseRunFieldMessage>(this, OnRunFieldClosed).DisposeWith(disposables);
        messenger.RegisterHandler<OpenRunFieldMessage>(this, OnRunFieldOpen).DisposeWith(disposables);
        messenger.RegisterHandler<KeyPressedMessage>(this, OnKeyPressed).DisposeWith(disposables);
        playback.DisposeWith(disposables);

        restartButton.Clicked += Restart;
        previousButton.Clicked += Previous;
        playButton.Clicked += TogglePlayback;
        nextButton.Clicked += Next;
        finishButton.Clicked += Finish;
        speedButton.Clicked += ChangeSpeed;

        BindTo(bar, x => (float)Math.Round((x.MouseEvent.X + 1f) / bar.Bounds.Width, 3), Button1Clicked);
        BindTo(bar, x => (float)Math.Round(((int)x.KeyEvent.Key - (int)D1) / 9f, 3), D2, D3, D4, D5, D6, D7, D8, D9);

        BindTo(bar, _ => Fraction - GetFractionPerClick(), CursorLeft);
        BindTo(bar, _ => Fraction + GetFractionPerClick(), CursorRight);
        BindTo(bar, _ => Fraction - GetExtraFractionPerClick(), CursorLeft | ShiftMask);
        BindTo(bar, _ => Fraction + GetExtraFractionPerClick(), CursorRight | ShiftMask);

        BindTo(bar, _ => RunModel.FractionRange.LowerValueOfRange, CursorLeft | CtrlMask, D1);
        BindTo(bar, _ => RunModel.FractionRange.UpperValueOfRange, CursorRight | CtrlMask, D0);

        viewModel.WhenAnyValue(x => x.SelectedRun.Fraction).BindTo(this, x => x.Fraction).DisposeWith(disposables);
    }

    private static float GetFractionPerClick()
    {
        return Settings.Default.FractionPerClick;
    }

    private static float GetExtraFractionPerClick()
    {
        return GetFractionPerClick() * 3;
    }

    private bool HasSelectedRun => viewModel.SelectedRun != RunModel.Empty;

    private void Restart() => SetFraction(RunModel.FractionRange.LowerValueOfRange);

    private void Previous() => SetFraction(Fraction - GetFractionPerClick());

    private void Next() => SetFraction(Fraction + GetFractionPerClick());

    private void Finish() => SetFraction(RunModel.FractionRange.UpperValueOfRange);

    private void SetFraction(float fraction)
    {
        if (HasSelectedRun)
        {
            viewModel.SelectedRun.Fraction = fraction;
        }
    }

    private void TogglePlayback()
    {
        if (!HasSelectedRun)
        {
            return;
        }
        if (playback.Disposable is null)
        {
            StartPlayback();
        }
        else
        {
            PausePlayback();
        }
    }

    private void StartPlayback()
    {
        if (Fraction >= RunModel.FractionRange.UpperValueOfRange)
        {
            SetFraction(RunModel.FractionRange.LowerValueOfRange);
        }
        playButton.Text = "Pause";
        playback.Disposable = Observable.Interval(TimeSpan.FromMilliseconds(100))
            .Subscribe(_ => Application.MainLoop.Invoke(AdvancePlayback));
    }

    private void AdvancePlayback()
    {
        if (!HasSelectedRun)
        {
            PausePlayback();
            return;
        }
        var next = Fraction + GetFractionPerClick() *
            (float)PlaybackSpeeds[playbackSpeedIndex];
        SetFraction(next);
        if (next >= RunModel.FractionRange.UpperValueOfRange)
        {
            PausePlayback();
        }
    }

    private void PausePlayback()
    {
        playback.Disposable = null;
        playButton.Text = "Play";
    }

    private void ChangeSpeed()
    {
        playbackSpeedIndex = (playbackSpeedIndex + 1) % PlaybackSpeeds.Length;
        speedButton.Text = $"{PlaybackSpeeds[playbackSpeedIndex]:0.##}x";
    }

    private void OnKeyPressed(KeyPressedMessage message)
    {
        if (message.Args.KeyEvent.Key == Space)
        {
            TogglePlayback();
            message.Args.Handled = true;
            return;
        }
        bar.OnKeyDown(message.Args.KeyEvent);
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
        restartButton.Clicked -= Restart;
        previousButton.Clicked -= Previous;
        playButton.Clicked -= TogglePlayback;
        nextButton.Clicked -= Next;
        finishButton.Clicked -= Finish;
        speedButton.Clicked -= ChangeSpeed;
        disposables.Dispose();
        base.Dispose(disposing);
    }

    private void OnRunFieldClosed(CloseRunFieldMessage msg)
    {
        PausePlayback();
        SetControlsVisible(false);
    }

    private void OnRunFieldOpen(OpenRunFieldMessage msg)
    {
        SetControlsVisible(true);
    }
}
