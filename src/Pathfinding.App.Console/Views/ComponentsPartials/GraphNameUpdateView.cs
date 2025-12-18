using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphNameUpdateView
{
    private readonly TextField nameField = new();
    private readonly Label nameLabel = new("Name");

    private void Initialize()
    {
        X = 0;
        Y = 0;
        Height = Dim.Percent(20);
        Width = Dim.Fill();
        Border = new Border()
        {
            BorderStyle = BorderStyle.None,
            Padding = new Thickness(0)
        };

        nameLabel.X = 1;
        nameLabel.Y = 1;
        nameLabel.Width = Dim.Percent(15);

        nameField.X = Pos.Percent(15) + 2;
        nameField.Y = 1;
        nameField.Width = Dim.Fill(1);

        Add(nameField, nameLabel);
    }
}