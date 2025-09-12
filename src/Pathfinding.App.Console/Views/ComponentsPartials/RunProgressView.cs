using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal partial class RunProgressView
{
    private readonly ProgressBar bar = new();
    private readonly Label leftLabel = new("-");
    private readonly Label rightLabel = new("+");

    private void Initialize()
    {
        X = 0;
        Y = Pos.Percent(95);
        Width = Dim.Percent(66);
        Height = Dim.Fill();
        var driver = Application.Driver;
        bar.ColorScheme = new()
        {
            Normal = driver.MakeAttribute(Color.DarkGray, Color.Black)
        };
        Border = new()
        {
            BorderBrush = Color.BrightYellow,
            BorderStyle = BorderStyle.Rounded
        };
        bar.Fraction = 0;

        bar.Width = Dim.Percent(94);
        bar.X = Pos.Right(leftLabel);
        bar.Y = Pos.Center();
        bar.ProgressBarStyle = ProgressBarStyle.Blocks;
        bar.ProgressBarFormat = ProgressBarFormat.Framed;

        leftLabel.WantContinuousButtonPressed = true;
        leftLabel.TextAlignment = TextAlignment.Centered;
        leftLabel.Y = Pos.Center();
        leftLabel.Width = Dim.Percent(3);
        leftLabel.X = 1;

        rightLabel.X = Pos.Right(bar) + 1;
        rightLabel.Y = Pos.Center();
        rightLabel.TextAlignment = TextAlignment.Centered;
        rightLabel.WantContinuousButtonPressed = true;
        rightLabel.Width = Dim.Percent(3);

        Add(rightLabel, bar, leftLabel);
        rightLabel.Visible = false;
        leftLabel.Visible = false;
        bar.Visible = false;
    }
}