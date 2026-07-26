using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class GraphSmoothLevelView
{
    private readonly RadioGroup smoothLevels = new();

    private void Initialize()
    {
        X = Pos.Percent(75);
        Y = 4;
        Width = Dim.Fill(1);
        Height = Dim.Fill(3);
        Border = new Border()
        {
            BorderStyle = BorderStyle.Rounded,
            Padding = new Thickness(0),
            Title = "Smooth"
        };

        smoothLevels.X = 1;
        smoothLevels.Y = 1;
        Add(smoothLevels);
    }
}
