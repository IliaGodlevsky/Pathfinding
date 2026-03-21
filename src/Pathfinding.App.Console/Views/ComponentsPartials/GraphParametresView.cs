using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphParametresView
{
    private readonly Label graphWidthLabel = new("Width");
    private readonly TextField graphWidthInput = new();
    private readonly Label graphLengthLabel = new("Length");
    private readonly TextField graphLengthInput = new();
    private readonly Label obstaclesLabel = new("Obstacles");
    private readonly TextField obstaclesInput = new();
    private readonly Label costRangeLabel = new("Range");
    private readonly TextField upperCostInput = new();
    private readonly TextField lowerCostInput = new();

    private void Initialize()
    {
        X = 1;
        Y = Pos.Percent(25) + 1;
        Width = Dim.Percent(35);
        Height = Dim.Percent(60);
        Border = new Border()
        {
            BorderStyle = BorderStyle.Rounded,
            Padding = new Thickness(0),
            Title = "Parametres"
        };

        graphWidthLabel.X = 1;
        graphWidthLabel.Y = 1;
        graphWidthLabel.Width = Dim.Percent(50, true);

        graphWidthInput.X = Pos.Right(graphWidthLabel) + 1;
        graphWidthInput.Y = 1;
        graphWidthInput.Width = Dim.Fill(1);

        graphLengthLabel.X = 1;
        graphLengthLabel.Y = Pos.Bottom(graphWidthLabel) + 1;
        graphLengthLabel.Width = Dim.Percent(50, true);

        graphLengthInput.X = Pos.Right(graphLengthLabel) + 1;
        graphLengthInput.Y = Pos.Bottom(graphWidthInput) + 1;
        graphLengthInput.Width = Dim.Fill(1);

        obstaclesLabel.X = 1;
        obstaclesLabel.Y = Pos.Bottom(graphLengthLabel) + 1;
        obstaclesLabel.Width = Dim.Percent(50, true);

        obstaclesInput.X = Pos.Right(obstaclesLabel) + 1;
        obstaclesInput.Y = Pos.Bottom(graphLengthInput) + 1;
        obstaclesInput.Width = Dim.Fill(1);

        costRangeLabel.Y = Pos.Bottom(obstaclesLabel) + 1;
        costRangeLabel.X = 1;
        costRangeLabel.Width = Dim.Percent(50, true);

        lowerCostInput.X = Pos.Right(costRangeLabel) + 1;
        lowerCostInput.Y = Pos.Bottom(obstaclesInput) + 1;
        lowerCostInput.Width = Dim.Percent(17);

        upperCostInput.X = Pos.Right(lowerCostInput) + 1;
        upperCostInput.Y = Pos.Bottom(obstaclesInput) + 1;
        upperCostInput.Width = Dim.Percent(17);

        graphWidthInput.KeyPress += KeyRestriction;
        graphLengthInput.KeyPress += KeyRestriction;
        obstaclesInput.KeyPress += KeyRestriction;
        upperCostInput.KeyPress += KeyRestriction;
        lowerCostInput.KeyPress += KeyRestriction;

        Add(graphWidthLabel, graphWidthInput,
            graphLengthLabel, graphLengthInput,
            obstaclesLabel, obstaclesInput,
            costRangeLabel, lowerCostInput, 
            upperCostInput);
    }

    private void KeyRestriction(KeyEventEventArgs args)
    {
        var keyChar = (char)args.KeyEvent.KeyValue;
        if (args.KeyEvent.Key == Key.Backspace ||
            args.KeyEvent.Key == Key.Delete ||
            args.KeyEvent.Key == Key.CursorLeft ||
            args.KeyEvent.Key == Key.CursorRight ||
            args.KeyEvent.Key == Key.Home ||
            args.KeyEvent.Key == Key.End ||
            char.IsDigit(keyChar))
        {
            return;
        }

        args.Handled = true;
    }
}