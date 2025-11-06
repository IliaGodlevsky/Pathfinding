using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphPanel : FrameView
{
    private void Initialize()
    {
        X = 0;
        Y = Pos.Percent(0);
        Width = Dim.Fill();
        Height = Dim.Percent(35);
        Border = new Border()
        {
            BorderStyle = BorderStyle.Rounded,
            BorderBrush = Color.Brown,
            Padding = new Thickness(1, 0, 1, 0),
            Title = "Graphs"
        };
    }
}