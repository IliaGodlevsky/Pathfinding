using Pathfinding.App.Console.Resources;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class GraphFieldView
    {
        private void Initialize()
        {
            X = 0;
            Y = 0;
            Width = Dim.Percent(66);
            Height = Dim.Percent(95);
            Border = new Border()
            {
                BorderBrush = Color.BrightYellow,
                BorderStyle = BorderStyle.Rounded,
                Title = Resource.GraphField
            };
        }
    }
}
