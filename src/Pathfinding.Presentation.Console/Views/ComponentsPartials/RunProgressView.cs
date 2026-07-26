using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal partial class RunProgressView
{
    private readonly ProgressBar bar = new();
    private readonly Button restartButton = new("<<");
    private readonly Button previousButton = new("<");
    private readonly Button playButton = new("Play ");
    private readonly Button nextButton = new(">");
    private readonly Button finishButton = new(">>");
    private readonly Button speedButton = new("1x");

    private void Initialize()
    {
        X = 0;
        Y = Pos.Percent(95) - 1;
        Width = Dim.Percent(66);
        Height = Dim.Fill(1);
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

        bar.Width = Dim.Fill(41);
        bar.X = 1;
        bar.Y = Pos.Center();
        bar.ProgressBarStyle = ProgressBarStyle.Continuous;
        bar.ProgressBarFormat = ProgressBarFormat.Framed;

        PositionButton(restartButton, Pos.Right(bar) + 1, 5);
        PositionButton(previousButton, Pos.Right(restartButton), 5);
        PositionButton(playButton, Pos.Right(previousButton), 9);
        PositionButton(nextButton, Pos.Right(playButton), 5);
        PositionButton(finishButton, Pos.Right(nextButton), 5);
        PositionButton(speedButton, Pos.Right(finishButton), 7);

        Add(bar, restartButton, previousButton, playButton,
            nextButton, finishButton, speedButton);
        SetControlsVisible(false);
    }

    private static void PositionButton(Button button, Pos x, int width)
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
