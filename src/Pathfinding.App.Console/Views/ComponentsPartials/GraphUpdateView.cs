using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphUpdateView
{
    private readonly Button updateButton = new("Update");
    private readonly Button cancelButton = new("Cancel");

    private void Initialize()
    {
        X = Pos.Center();
        Y = Pos.Center();
        Width = Dim.Fill();
        Height = Dim.Fill();
        Visible = false;
        Border = new Border()
        {
            BorderStyle = BorderStyle.None,
            BorderThickness = new Thickness(0)
        };

        updateButton.X = Pos.Percent(35);
        updateButton.Y = Pos.Percent(85) + 1;
        cancelButton.X = Pos.Right(updateButton) + 2;
        cancelButton.Y = Pos.Percent(85) + 1;
        Add(updateButton, cancelButton);
    }
}