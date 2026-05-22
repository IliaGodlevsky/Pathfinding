using Pathfinding.Presentation.Console.Resources;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal partial class RunsListView
{
    private void Initialize()
    {
        Y = 1;
        Width = Dim.Percent(50);
        Height = Dim.Fill(1);
        Border = new Border()
        {
            BorderStyle = BorderStyle.Rounded,
            Title = Resource.Algorithms,
            BorderThickness = new Thickness(0)
        };
    }
}