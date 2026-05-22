using Pathfinding.Presentation.Console.Resources;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class GraphExportButton : Button
{
    private void Initialize()
    {
        Text = Resource.Save;
    }
}