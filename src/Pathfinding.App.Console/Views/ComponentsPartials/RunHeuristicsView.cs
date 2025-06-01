using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunHeuristicsView : FrameView
{
    private void Initialize()
    {
        X = 0;
        Y = Pos.Percent(20) + 1;
        Height = Dim.Percent(35);
        Width = Dim.Percent(30);
        Border = new Border()
        {
            BorderStyle = BorderStyle.Rounded,
            Title = "Heuristics"
        };
        Visible = false;
    }
}