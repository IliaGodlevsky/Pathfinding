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
        var childs = children
            .OrderBy(x => x.Metadata[MetadataKeys.Order])
            .Select(x => x.Value)
            .ToArray();
        float widthPercent = 100f / childs.Length;
        for (int i = 0; i < childs.Length; i++)
        {
            childs[i].X = Pos.Percent(i * widthPercent);
            childs[i].Width = Dim.Percent(widthPercent);
        }
        Add(childs);
    }
}
