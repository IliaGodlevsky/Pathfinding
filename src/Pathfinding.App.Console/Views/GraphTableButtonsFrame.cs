using Autofac.Features.AttributeFilters;
using Pathfinding.App.Console.Injection;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class GraphTableButtonsFrame : FrameView
    {
        public GraphTableButtonsFrame([KeyFilter(KeyFilters.GraphTableButtons)] View[] children)
        {
            Initialize();
            Add(children);
        }
    }
}
