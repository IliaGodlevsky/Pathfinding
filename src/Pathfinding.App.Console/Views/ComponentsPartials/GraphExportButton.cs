using Pathfinding.App.Console.Resources;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class GraphExportButton : Button
    {
        private void Initialize()
        {
            Text = Resource.Save;
        }
    }
}
