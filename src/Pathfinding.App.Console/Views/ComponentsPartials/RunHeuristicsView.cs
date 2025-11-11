using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunHeuristicsView : FrameView
{
    private void Initialize()
    {
        Height = Dim.Percent(25);
        Width = Dim.Percent(30);
        Border = new Border()
        {
            BorderStyle = BorderStyle.Rounded,
            Title = "Heuristics"
        };
        Visible = false;
    }
}