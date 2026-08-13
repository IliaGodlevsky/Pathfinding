using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.Presentation.Console.Extensions;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Messages.View;
using Pathfinding.Presentation.Console.Models;
using Pathfinding.Presentation.Console.ViewModels.Interface;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;
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
    private static readonly float[] PlaybackSpeeds = [0.25f, 0.5f, 1, 2, 4, 6];

    private readonly IRunFieldViewModel viewModel;
    private readonly CompositeDisposable disposables = [];
    private readonly SerialDisposable playback = new();

    private InclusiveValueRange<int> PlaybackIndexRange { get; } = (PlaybackSpeeds.Length - 1, 0);

    private int playbackSpeedIndex = 2;
    private int PlaybackSpeedIndex
    {
        get => playbackSpeedIndex; 
        set => playbackSpeedIndex = PlaybackIndexRange.ReturnInRange(value, ReturnOptions.Cycle);
    }

    private float PlaybackSpeed => PlaybackSpeeds[PlaybackSpeedIndex];

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

        BindTo(restartButton, _ => RunModel.FractionRange.LowerValueOfRange, Button1Clicked);
        BindTo(previousButton, _ => Fraction - GetFractionPerClick(), Button1Clicked);
        BindTo(nextButton, _ => Fraction + GetFractionPerClick(), Button1Clicked);
        BindTo(finishButton, _ => RunModel.FractionRange.UpperValueOfRange, Button1Clicked);
        playButton.Clicked += TogglePlayback;
        speedButton.MouseClick += ChangeSpeed;

        BindTo(bar, x => (float)Math.Round((x.MouseEvent.X + 1f) / bar.Bounds.Width, 3), Button1Clicked);
        BindTo(bar, x => (float)Math.Round(((int)x.KeyEvent.Key - (int)D1) / 9f, 3), D2, D3, D4, D5, D6, D7, D8, D9);

        BindTo(bar, _ => Fraction - GetFractionPerClick(), CursorLeft);
        BindTo(bar, _ => Fraction + GetFractionPerClick(), CursorRight);
        BindTo(bar, _ => Fraction - GetExtraFractionPerClick(), CursorLeft | ShiftMask);
        BindTo(bar, _ => Fraction + GetExtraFractionPerClick(), CursorRight | ShiftMask);

        foreach (var button in new[] { nextButton, playButton, previousButton, finishButton, restartButton })
        {
            BindTo(button, _ => Fraction + GetFractionPerClick() * PlaybackSpeed, WheeledUp);
            BindTo(button, _ => Fraction - GetFractionPerClick() * PlaybackSpeed, WheeledDown);
        }

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

    private void SetFraction(float fraction)
    {
        if (HasSelectedRun)
        {
            viewModel.SelectedRun.Fraction = fraction;
        }
    }

    private void TogglePlayback()
    {
        if (HasSelectedRun)
        {
            if (playback.Disposable is null)
            {
                if (Fraction >= RunModel.FractionRange.UpperValueOfRange)
                {
                    SetFraction(RunModel.FractionRange.LowerValueOfRange);
                }
                playButton.Text = "Pause";
                playback.Disposable = Observable.Interval(TimeSpan.FromMilliseconds(75))
                    .Subscribe(_ => Application.MainLoop.Invoke(AdvancePlayback));
            }
            else
            {
                PausePlayback();
            }
        }
    }

    private void AdvancePlayback()
    {
        if (!HasSelectedRun)
        {
            PausePlayback();
            return;
        }
        var next = Fraction + GetFractionPerClick() * PlaybackSpeed;
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

    private void ChangeSpeed(MouseEventArgs e)
    {
        PlaybackSpeedIndex += e.MouseEvent.Flags switch
        {
            Button1Clicked => 1,
            Button3Clicked => -1,
            _ => 0
        };
        speedButton.Text = $"{PlaybackSpeed:0.##}x";
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
            .Where(x => HasSelectedRun && flags.Any(z => predicate(z, x)))
            .Select(function)
            .BindTo(viewModel, x => x.SelectedRun.Fraction)
            .DisposeWith(disposables);
    }

    protected override void Dispose(bool disposing)
    {
        playButton.Clicked -= TogglePlayback;
        speedButton.MouseClick -= ChangeSpeed;
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
