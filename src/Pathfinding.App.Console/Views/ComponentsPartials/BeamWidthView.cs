using Pathfinding.App.Console.Resources;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class BeamWidthView : FrameView
{
    private readonly Label beamWidthLabel = new("Width   ");
    private readonly TextField beamWidthTextField = new();

    private void Initialize()
    {
        X = 0;
        Y = Pos.Percent(58);
        Height = Dim.Percent(42);
        Width = Dim.Percent(30);
        Border = new Border()
        {
            BorderStyle = BorderStyle.Rounded,
            Title = Resource.BeamWidth
        };
        Visible = false;

        beamWidthLabel.Y = 1;
        beamWidthLabel.X = 1;
        beamWidthTextField.X = Pos.Right(beamWidthLabel) + 1;
        beamWidthTextField.Y = 1;
        beamWidthTextField.Width = Dim.Percent(35);

        beamWidthTextField.KeyPress += OnKeyPress;
        Add(beamWidthLabel, beamWidthTextField);
    }

    private static void OnKeyPress(KeyEventEventArgs args)
    {
        if (args.KeyEvent.Key == Key.Backspace ||
            args.KeyEvent.Key == Key.Delete ||
            args.KeyEvent.Key == Key.CursorLeft ||
            args.KeyEvent.Key == Key.CursorRight ||
            args.KeyEvent.Key == Key.Home ||
            args.KeyEvent.Key == Key.End)
        {
            return;
        }

        if (char.IsDigit((char)args.KeyEvent.KeyValue))
        {
            return;
        }

        args.Handled = true;
    }
}
