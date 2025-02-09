using Autofac.Features.AttributeFilters;
using Pathfinding.App.Console.Injection;
using Terminal.Gui;

namespace Pathfinding.App.Console.View
{
    internal sealed partial class GraphPanel : FrameView
    {
        public GraphPanel([KeyFilter(KeyFilters.GraphPanel)] Terminal.Gui.View[] children)
        {
            Initialize();
            Add(children);
        }
    }
}
