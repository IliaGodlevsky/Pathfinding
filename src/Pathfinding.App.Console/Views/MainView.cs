using Autofac.Features.AttributeFilters;
using Pathfinding.App.Console.Injection;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed class MainView : Window
    {
        public MainView([KeyFilter(KeyFilters.MainWindow)] View[] children)
        {
            X = 0;
            Y = 0;
            Height = Dim.Fill();
            Width = Dim.Fill();
            Border = new() { DrawMarginFrame = false, BorderThickness = new(0) };
            Add(children);
            Loaded += OnActivate;
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
}
