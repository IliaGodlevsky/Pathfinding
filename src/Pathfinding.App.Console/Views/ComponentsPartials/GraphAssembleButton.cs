using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class GraphAssembleButton
    {
        private void Initialize()
        {
            Text = "New";
            X = 0;
            Y = 0;
            Width = Dim.Percent(16.67f);
        }
    }
}
