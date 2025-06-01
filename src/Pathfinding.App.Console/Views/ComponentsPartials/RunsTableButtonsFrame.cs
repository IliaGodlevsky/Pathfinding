using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal partial class RunsTableButtonsFrame
{
    private void Initialize()
    {
        Border = new Border()
        {
            BorderStyle = BorderStyle.Rounded,
            DrawMarginFrame = false,
            Padding = new Thickness(0)
        };
        X = 0;
        Y = Pos.Percent(90);
        Width = Dim.Fill();
        Height = Dim.Fill();
    }
}