using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class GraphNeighborhoodUpdateView
    {
        private readonly RadioGroup neighborhoods = new();

        private void Initialize()
        {
            X = Pos.Percent(15) + 1;
            Y = Pos.Percent(25) + 1;
            Width = Dim.Percent(25);
            Height = Dim.Percent(40);
            Border = new Border()
            {
                BorderStyle = BorderStyle.Rounded,
                Padding = new Thickness(0),
                Title = "Neighbors"
            };
            neighborhoods.X = 1;
            neighborhoods.Y = 1;
            Add(neighborhoods);
        }
    }
}
