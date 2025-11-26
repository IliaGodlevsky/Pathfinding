using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunHeuristicsView : FrameView
{
    private void Initialize()
    {
        Height = Dim.Percent(34);
        Width = Dim.Fill();
        Border = new Border()
        {
            BorderStyle = BorderStyle.Rounded,
            Title = "Heuristics"
        };
        Visible = false;
    }
}