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

using static Terminal.Gui.MouseFlags;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunProgressView : FrameView
{
    private const float FractionPerClick = 0.015f;
    private const float ExtraFractionPerClick = FractionPerClick * 3;

    private readonly CompositeDisposable disposables = [];
    private readonly IRunFieldViewModel viewModel;

    public RunProgressView(
        [KeyFilter(KeyFilters.Views)] IMessenger messenger,
        IRunFieldViewModel viewModel)
    {
        Initialize();
        messenger.Register<CloseRunFieldMessage>(this, OnRunFieldClosed);
        messenger.Register<OpenRunFieldMessage>(this, OnRunFieldOpen);
        this.viewModel = viewModel;
        
        BindTo(leftLabel, x => bar.Fraction - FractionPerClick, Button1Pressed);
        BindTo(leftLabel, x => bar.Fraction - ExtraFractionPerClick, Button1Pressed, ButtonCtrl);
        BindTo(leftLabel, x => bar.Fraction - FractionPerClick, WheeledDown);
        BindTo(leftLabel, x => bar.Fraction + FractionPerClick, WheeledUp);
        BindTo(leftLabel, x => RunModel.FractionRange.LowerValueOfRange, Button2Clicked);
        BindTo(rightLabel, x => bar.Fraction + FractionPerClick, Button1Pressed);
        BindTo(rightLabel, x => bar.Fraction + ExtraFractionPerClick, Button1Pressed, ButtonCtrl);
        BindTo(rightLabel, x => bar.Fraction - FractionPerClick, WheeledDown);
        BindTo(rightLabel, x => bar.Fraction + FractionPerClick, WheeledUp);
        BindTo(rightLabel, x => RunModel.FractionRange.UpperValueOfRange, Button2Clicked);
        BindTo(bar, x => (float)Math.Round((float)(x.MouseEvent.X + 1) / bar.Bounds.Width, 3), Button1Clicked);
        viewModel.WhenAnyValue(x => x.SelectedRun.Fraction).BindTo(bar, x => x.Fraction);
    }

    private void BindTo(View view, Func<MouseEventArgs, float> function, params MouseFlags[] flags)
    {
        view.Events().MouseClick
            .Where(x => viewModel.SelectedRun != RunModel.Empty
                && flags.All(z => x.MouseEvent.Flags.HasFlag(z)))
            .Select(function)
            .BindTo(viewModel, x => x.SelectedRun.Fraction);
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
