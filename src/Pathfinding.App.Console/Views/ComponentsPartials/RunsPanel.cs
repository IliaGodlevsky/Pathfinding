using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class RunsPanel
    {
        private void Initialize()
        {
            X = 0;
            Y = Pos.Percent(50);
            Width = Dim.Fill();
            Height = Dim.Fill();
            Border = new Border()
            {
                BorderStyle = BorderStyle.Rounded,
                Padding = new Thickness(1, 0, 1, 0),
                BorderBrush = Color.BrightCyan,
                Title = "Runs"
            };
        }
    }
}
