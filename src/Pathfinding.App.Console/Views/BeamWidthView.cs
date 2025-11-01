using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Infrastructure.Business.Algorithms;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class BeamWidthView : FrameView
{
    private const int DefaultBeamWidth = BeamSearchAlgorithm.DefaultBeamWidth;

    private readonly IRequireBeamWidthViewModel viewModel;
    private readonly CompositeDisposable disposables = [];

    public BeamWidthView(
        [KeyFilter(KeyFilters.Views)] IMessenger messenger,
        IRequireBeamWidthViewModel viewModel)
    {
        Initialize();
        this.viewModel = viewModel;

        beamWidthTextField.Events().TextChanging
            .DistinctUntilChanged()
            .Select(x => int.TryParse(x.NewText.ToString(), out var value) ? value : default(int?))
            .BindTo(viewModel, x => x.BeamWidth)
            .DisposeWith(disposables);

        viewModel.Events().PropertyChanged
            .Where(x => x.PropertyName == nameof(IRequireBeamWidthViewModel.BeamWidth))
            .Do(_ => Application.MainLoop.Invoke(() =>
            {
                var propertyValue = viewModel.BeamWidth;
                var parsed = int.TryParse(beamWidthTextField.Text.ToString(), out var textValue);
                if (!parsed || textValue != propertyValue)
                {
                    beamWidthTextField.Text = propertyValue?.ToString() ?? string.Empty;
                }
            }))
            .Subscribe()
            .DisposeWith(disposables);

        messenger.RegisterHandler<OpenBeamWidthViewMessage>(this, OnOpen).DisposeWith(disposables);
        messenger.RegisterHandler<CloseBeamWidthViewMessage>(this, OnClose).DisposeWith(disposables);
        messenger.RegisterHandler<CloseRunCreateViewMessage>(this, OnRunCreateClosed).DisposeWith(disposables);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            disposables.Dispose();
        }

        base.Dispose(disposing);
    }

    private void OnOpen(OpenBeamWidthViewMessage _)
    {
        viewModel.BeamWidth = DefaultBeamWidth;
        beamWidthTextField.Text = DefaultBeamWidth.ToString();
        Visible = true;
    }

    private void OnClose(CloseBeamWidthViewMessage _)
    {
        Close();
    }

    private void OnRunCreateClosed(CloseRunCreateViewMessage _)
    {
        Close();
    }

    private void Close()
    {
        viewModel.BeamWidth = null;
        beamWidthTextField.Text = string.Empty;
        Visible = false;
    }
}
