using Autofac.Features.AttributeFilters;
using Pathfinding.App.Console.Injection;
using Terminal.Gui;

namespace Pathfinding.App.Console.View
{
    internal sealed partial class GraphTableButtonsFrame : FrameView
    {
        public GraphTableButtonsFrame([KeyFilter(KeyFilters.GraphTableButtons)] Terminal.Gui.View[] children)
        {
            Initialize();
            Add(children);
        }
    }
}
