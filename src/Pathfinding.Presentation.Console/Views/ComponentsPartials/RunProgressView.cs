using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal partial class RunProgressView
{
    private readonly ProgressBar bar = new();
    private readonly Label restartButton = new("<<");
    private readonly Label previousButton = new("<");
    private readonly Label playButton = new("Play ");
    private readonly Label nextButton = new(">");
    private readonly Label finishButton = new(">>");
    private readonly Label speedButton = new("1x");

    private void Initialize()
    {
        X = 0;
        Y = Pos.Percent(95) - 1;
        Width = Dim.Percent(66);
        Height = Dim.Fill(1);
        bar.ColorScheme = new()
        {
            Normal = Application.Driver.MakeAttribute(Color.DarkGray, Color.Black)
        };
        Border = new()
        {
            BorderBrush = Color.BrightYellow,
            BorderStyle = BorderStyle.Rounded
        };
        bar.Fraction = 0;

        bar.Width = Dim.Fill(31);
        bar.X = 1;
        bar.Y = Pos.Center();
        bar.ProgressBarStyle = ProgressBarStyle.Continuous;
        bar.ProgressBarFormat = ProgressBarFormat.Framed;

        PositionButton(restartButton, Pos.Right(bar) + 1, 4);
        PositionButton(previousButton, Pos.Right(restartButton), 4);
        PositionButton(playButton, Pos.Right(previousButton), 8);
        PositionButton(nextButton, Pos.Right(playButton), 4);
        PositionButton(finishButton, Pos.Right(nextButton), 4);
        PositionButton(speedButton, Pos.Right(finishButton), 5);

        Add(bar, restartButton, previousButton, playButton,
            nextButton, finishButton, speedButton);
        SetControlsVisible(false);
    }

    private static void PositionButton(View button, Pos x, int width)
    {
        button.X = x;
        button.Y = Pos.Center();
        button.Width = width;
    }

    private void SetControlsVisible(bool visible)
    {
        foreach (var control in Subviews)
        {
            control.Visible = visible;
        }
    }
}
