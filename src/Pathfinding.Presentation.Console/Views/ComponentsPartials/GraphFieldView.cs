using Pathfinding.Presentation.Console.Resources;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class GraphFieldView
{
    private void Initialize()
    {
        X = 0;
        Y = Pos.Percent(7);
        Width = Dim.Percent(66);
        Height = Dim.Percent(90);
        Border = new Border()
        {
            BorderBrush = Color.BrightYellow,
            BorderStyle = BorderStyle.Rounded,
            Title = Resource.GraphField
        };
    }
}