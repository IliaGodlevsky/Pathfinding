using Pathfinding.App.Console.Resources;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class GraphImportButton
    {
        private void Initialize()
        {
            Text = Resource.Load;
            Y = 0;
            X = Pos.Percent(66.68f);
            Width = Dim.Percent(16.67f);
        }
    }
}