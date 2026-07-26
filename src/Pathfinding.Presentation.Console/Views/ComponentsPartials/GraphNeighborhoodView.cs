using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class GraphNeighborhoodView
{
    private readonly RadioGroup neighborhoods = new();

    private void Initialize()
    {
        X = Pos.Percent(34);
        Y = 12;
        Width = Dim.Percent(32);
        Height = Dim.Fill(3);
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
