using Autofac.Features.AttributeFilters;
using Pathfinding.Presentation.Console.Injection;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed class MainView : Window, IAsyncDisposable
{
    private readonly StatusBar statusBar;

    public MainView([KeyFilter(KeyFilters.MainWindow)] View[] children)
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
            new(Key.F2, "~F2~ Legend", ShowGraphLegend)
        ]);

        Add(children);
        Add(statusBar);
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
        base.Dispose(disposing);
    }

    private static void ShowKeyboardHelp()
    {
        using var dialog = new KeyboardHelpDialog();
        Application.Run(dialog);
    }

    private static void ShowGraphLegend()
    {
        using var dialog = new GraphLegendDialog();
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
