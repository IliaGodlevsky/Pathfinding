using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal partial class RunStepRulesView
{
    private readonly RadioGroup stepRules = new();

    private void Initialize()
    {
        stepRules.X = 1;
        stepRules.Y = 0;
        Height = Dim.Percent(20);
        Width = Dim.Fill();
        Border = new Border()
        {
            BorderStyle = BorderStyle.Rounded,
            Title = "Step rules"
        };
        Visible = false;
        Add(stepRules);
    }
}