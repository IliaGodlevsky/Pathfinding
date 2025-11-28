using Pathfinding.App.Console.Resources;
using System.Globalization;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunsPopulateView
{
    private readonly Label weightLabel = new("Weight");
    private readonly TextField weightTextField = new();
    private readonly Label toWeightLabel = new("To wieght");
    private readonly TextField toWeightTextField = new();
    private readonly Label stepLabel = new("Step");
    private readonly TextField stepTextField = new();

    private void Initialize()
    {
        Height = Dim.Percent(40);
        Width = Dim.Fill();
        Border = new Border()
        {
            BorderStyle = BorderStyle.Rounded,
            Title = Resource.RunsPopulate
        };
        Visible = false;

        weightLabel.Y = 1;
        weightLabel.X = 1;
        weightTextField.X = Pos.Right(weightLabel) + 4;
        weightTextField.Y = 1;
        weightTextField.Width = Dim.Percent(37);

        toWeightLabel.Y = Pos.Bottom(weightLabel) + 1;
        toWeightLabel.X = 1;
        toWeightTextField.X = Pos.Right(toWeightLabel) + 1;
        toWeightTextField.Y = Pos.Bottom(weightTextField) + 1;
        toWeightTextField.Width = Dim.Percent(37);

        stepLabel.Y = Pos.Bottom(toWeightLabel) + 1;
        stepLabel.X = 1;
        stepTextField.X = Pos.Right(stepLabel) + 6;
        stepTextField.Y = Pos.Bottom(toWeightLabel) + 1;
        stepTextField.Width = Dim.Percent(37);

        weightTextField.KeyPress += (args) => KeyRestriction(args, weightTextField);
        toWeightTextField.KeyPress += (args) => KeyRestriction(args, toWeightTextField);
        stepTextField.KeyPress += (args) => KeyRestriction(args, stepTextField);
        Add(weightLabel, weightTextField, toWeightLabel,
            toWeightTextField, stepLabel, stepTextField);
    }

    private static void KeyRestriction(KeyEventEventArgs args, TextField field)
    {
        var decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        var keyChar = (char)args.KeyEvent.KeyValue;
        if (args.KeyEvent.Key == Key.Backspace ||
            args.KeyEvent.Key == Key.Delete ||
            args.KeyEvent.Key == Key.CursorLeft ||
            args.KeyEvent.Key == Key.CursorRight ||
            args.KeyEvent.Key == Key.Home ||
            args.KeyEvent.Key == Key.End)
        {
            return;
        }
        if (char.IsDigit(keyChar))
        {
            if (field.Text.Length + 1 <= 4)
            {
                return;
            }
        }
        if (keyChar.ToString() == decimalSeparator
            && !field.Text.ToString().Contains(decimalSeparator))
        {
            return;
        }

        args.Handled = true;
    }
}