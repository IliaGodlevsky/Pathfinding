using Pathfinding.App.Console.Resources;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal partial class RunsListView
{
    private void Initialize()
    {
        X = 0;
        Y = 1;
        Width = Dim.Percent(25);
        Height = Dim.Percent(62);
        Border = new Border()
        {
            BorderStyle = BorderStyle.Rounded,
            Title = Resource.Algorithms,
            BorderThickness = new Thickness(0)
        };
    }
}