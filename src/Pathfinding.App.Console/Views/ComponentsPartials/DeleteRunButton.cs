using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunDeleteButton
{
    private void Initialize()
    {
        Text = "Delete";
        X = Pos.Percent(66);
        Y = 0;
        Width = Dim.Percent(34);
    }
}