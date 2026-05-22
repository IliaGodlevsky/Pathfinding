using Autofac.Features.AttributeFilters;
using Pathfinding.Presentation.Console.Injection;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class GraphPanel : FrameView
{
    public GraphPanel([KeyFilter(KeyFilters.GraphPanel)] View[] children)
    {
        Initialize();
        Add(children);
    }
}
