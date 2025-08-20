using Autofac.Features.AttributeFilters;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.ViewModels.Interface;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed class RunCreateView : FrameView
{
    private readonly Button createButton = new("Create");
    private readonly Button cancelButton = new("Cancel");
    private readonly FrameView buttonFrame = new();

    private readonly CompositeDisposable disposables = [];

    public RunCreateView(
        IRunCreateViewModel viewModel,
        [KeyFilter(KeyFilters.RunCreateView)] View[] children)
    {
        Initialize();

        Add(children);
        Add(buttonFrame);
        cancelButton.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Do(x => Visible = false)
            .Subscribe()
            .DisposeWith(disposables);
        viewModel.CreateRunCommand.CanExecute
            .BindTo(createButton, x => x.Enabled)
            .DisposeWith(disposables);
        createButton.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Select(x => Unit.Default)
            .Do(x => Visible = false)
            .InvokeCommand(viewModel, x => x.CreateRunCommand)
            .DisposeWith(disposables);
        foreach (var child in children)
        {
            this.Events().VisibleChanged
                .Select(x => Visible)
                .BindTo(child, x => x.Visible)
                .DisposeWith(disposables);
        }
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        base.Dispose(disposing);
    }

    private void Initialize()
    {
        cancelButton.X = Pos.Percent(65);
        cancelButton.Y = 0;
        createButton.X = Pos.Percent(15);
        createButton.Y = 0;
        buttonFrame.Border = new()
        {
            BorderStyle = BorderStyle.Rounded
        };
        buttonFrame.X = 0;
        buttonFrame.Width = Dim.Fill();
        buttonFrame.Height = Dim.Fill();
        buttonFrame.Y = Pos.Percent(90);
        buttonFrame.Add(createButton, cancelButton);
        X = Pos.Center();
        Y = Pos.Center();
        Width = Dim.Fill();
        Height = Dim.Fill();
        Visible = false;
        Border = new()
        {
            BorderStyle = BorderStyle.None,
            BorderThickness = new(0)
        };
    }
}
