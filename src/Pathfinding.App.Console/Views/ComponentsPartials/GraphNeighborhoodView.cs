using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphNeighborhoodView
{
    private readonly RadioGroup neighborhoods = new();

    private void Initialize()
    {
        X = Pos.Percent(35) + 1;
        Y = Pos.Percent(25) + 1;
        Width = Dim.Percent(35);
        Height = Dim.Percent(55);
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