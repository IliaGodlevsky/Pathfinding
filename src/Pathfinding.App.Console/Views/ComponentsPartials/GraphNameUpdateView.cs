using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class GraphNameUpdateView
    {
        private readonly TextField nameField = new TextField();
        private readonly Label nameLabel = new Label("Name");

        private void Initialize()
        {
            X = 1;
            Y = 1;
            Height = Dim.Percent(15, true);
            Width = Dim.Fill(3);
            Border = new Border()
            {
                BorderStyle = BorderStyle.None,
                Padding = new Thickness(0)
            };

            nameLabel.X = 1;
            nameLabel.Y = 1;
            nameLabel.Width = Dim.Percent(15);

            nameField.X = Pos.Percent(15) + 1;
            nameField.Y = 1;
            nameField.Width = Dim.Fill();

            Add(nameField, nameLabel);
        }
    }
}
