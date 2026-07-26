using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class GraphNeighborhoodView
{
    private readonly RadioGroup neighborhoods = new();

    private void Initialize()
    {
        X = Pos.Percent(40);
        Y = 4;
        Width = Dim.Percent(33);
        Height = Dim.Percent(35);
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
