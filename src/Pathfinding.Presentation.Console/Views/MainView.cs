using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Messages.ViewModel.ValueMessages;
using Pathfinding.Presentation.Console.Models;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed class MainView : Window, IAsyncDisposable
{
    private readonly StatusBar statusBar;
    private readonly CompositeDisposable disposables = [];
    private RunInfoModel[] selectedRuns = [];

    public MainView(
        [KeyFilter(KeyFilters.MainWindow)] View[] children,
        [KeyFilter(KeyFilters.ViewModels)] IMessenger messenger)
    {
        X = 0;
        Y = 0;
        Height = Dim.Fill();
        Width = Dim.Fill();
        Border = new() 
        { 
            DrawMarginFrame = false, 
            BorderThickness = new(0) 
        };
        statusBar = new(
        [
            new(Key.F1, "~F1~ Help", ShowKeyboardHelp),
            new(Key.F2, "~F2~ Legend", ShowGraphLegend),
            new(Key.F3, "~F3~ Compare runs  |  Ctrl+A Select all", ShowRunComparison)
        ]);

        Add(children);
        Add(statusBar);
        messenger.RegisterHandler<RunsSelectedMessage>(this,
            message => selectedRuns = message.Value).DisposeWith(disposables);
        Loaded += OnActivate;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        Loaded -= OnActivate;
        disposables.Dispose();
        base.Dispose(disposing);
    }

    private static void ShowKeyboardHelp()
    {
        MessageBox.Query(
            "Keyboard shortcuts",
            "F1       Show this help\n" +
            "Tab      Move focus\n" +
            "Enter    Activate the selected graph\n" +
            "Ctrl+A   Select all rows in the focused table\n" +
            "Ctrl+R   Restore the default run order\n" +
            "Arrows   Move through tables and fields\n\n" +
            "Tip: click a run-table column heading to sort it.",
            "Ok");
    }

    private static void ShowGraphLegend()
    {
        using var dialog = new GraphLegendDialog();
        Application.Run(dialog);
    }

    private void ShowRunComparison()
    {
        if (selectedRuns.Length < 2)
        {
            MessageBox.Query(
                "Compare runs",
                "Select at least two runs to compare.",
                "Ok");
            return;
        }

        using var dialog = new RunComparisonDialog(selectedRuns);
        Application.Run(dialog);
    }

    private void OnActivate()
    {
        var driver = Application.Driver;
        var backgroundColor = Enum.Parse<Color>(Settings.Default.BackgroundColor);
        var foregroundColor = Enum.Parse<Color>(Settings.Default.ForegroundColor);
        var attribute = driver.MakeAttribute(foregroundColor, backgroundColor);
        Colors.ColorSchemes["Base"].Normal = attribute;
    }
}
