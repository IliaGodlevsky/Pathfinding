using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphAssembleView
{
    private readonly Button createButton = new("Create");
    private readonly Button cancelButton = new("Cancel");
    private readonly FrameView buttonsFrame = new();

    private void Initialize()
    {
        buttonsFrame.Border = new()
        {
            BorderStyle = BorderStyle.Rounded
        };
        buttonsFrame.X = 0;
        buttonsFrame.Y = Pos.Percent(90);
        buttonsFrame.Width = Dim.Fill();
        buttonsFrame.Height = Dim.Fill();
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

        cancelButton.X = Pos.Percent(65);
        cancelButton.Y = 0;
        createButton.X = Pos.Percent(15);
        createButton.Y = 0;
        buttonsFrame.Add(createButton, cancelButton);
        Add(buttonsFrame);
    }
}