using Pathfinding.Presentation.Console.Models;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed class GraphLegendDialog : Dialog
{
    private readonly Button closeButton = new("Close");

    public GraphLegendDialog()
    {
        Title = "Graph legend";
        Width = 48;
        Height = 18;

        Add(new Label("Graph editing") { X = 1, Y = 0 });
        AddLegendItem(1, "Regular vertex / traversal cost", VertexView<GraphVertexModel>.RegularColor);
        AddLegendItem(2, "Obstacle", VertexView<GraphVertexModel>.ObstacleColor);
        AddLegendItem(3, "Source", VertexView<GraphVertexModel>.SourceColor);
        AddLegendItem(4, "Target", VertexView<GraphVertexModel>.TargetColor);
        AddLegendItem(5, "Transit point", VertexView<GraphVertexModel>.TransitColor);

        Add(new Label("Run visualization") { X = 1, Y = 7 });
        AddLegendItem(8, "Enqueued for exploration", VertexView<GraphVertexModel>.EnqueuedColor);
        AddLegendItem(9, "Visited", VertexView<GraphVertexModel>.VisitedColor);
        AddLegendItem(10, "Final path", VertexView<GraphVertexModel>.PathColor);
        AddLegendItem(11, "Overlapping path", VertexView<GraphVertexModel>.CrossedPathColor);

        closeButton.Clicked += OnClose;
        AddButton(closeButton);
    }

    private void AddLegendItem(int row, string description, ColorScheme color)
    {
        var marker = new Label("██")
        {
            X = 1,
            Y = row,
            Width = 2,
            ColorScheme = color
        };
        var label = new Label(description)
        {
            X = Pos.Right(marker) + 2,
            Y = row,
            Width = Dim.Fill(1)
        };
        Add(marker, label);
    }

    private static void OnClose()
    {
        Application.RequestStop();
    }

    protected override void Dispose(bool disposing)
    {
        closeButton.Clicked -= OnClose;
        base.Dispose(disposing);
    }
}
