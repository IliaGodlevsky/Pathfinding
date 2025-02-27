using Pathfinding.App.Console.Resources;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class GraphDeleteButton
    {
        private void Initialize()
        {
            Text = Resource.DeleteGraph;
            X = Pos.Percent(83.65f);
            Y = 0;
            Width = Dim.Percent(16.65f);
        }
    }
}
