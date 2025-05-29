using Autofac.Features.AttributeFilters;
using Autofac.Features.Metadata;
using Pathfinding.App.Console.Injection;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphTableButtonsFrame : FrameView
{
    public GraphTableButtonsFrame(
        [KeyFilter(KeyFilters.GraphTableButtons)] Meta<Button>[] children)
    {
        Initialize();
        var kids = children
            .OrderBy(x => x.Metadata[MetadataKeys.Order])
            .Select(x => x.Value)
            .ToArray<View>();
        var widthPercent = 100f / kids.Length;
        for (var i = 0; i < kids.Length; i++)
        {
            kids[i].X = Pos.Percent(i * widthPercent);
            kids[i].Width = Dim.Percent(widthPercent);
        }
        Add(kids);
    }
}
