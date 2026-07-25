using Pathfinding.Domain.Enums;
using Pathfinding.Presentation.Console.Extensions;
using System.Data;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class GraphsTableView : TableView
{
    private const string IdCol = "Id";
    private const string NameCol = "Name";
    private const string WidthCol = "Width";
    private const string LengthCol = "Length";
    private const string NeighborsCol = "Neighbors";
    private const string SmoothCol = "Smooth";
    private const string ObstaclesCol = "Obstacles";
    private const string CostRangeCol = "Range";
    private const string StatusCol = "Status";

    private readonly DataTable table = new();
    private readonly int headerLinesConsumed;

    public GraphsTableView()
    {
        table.Columns.AddRange(
        [
            new(IdCol, typeof(int)),
            new(NameCol, typeof(string)),
            new(WidthCol, typeof(int)),
            new(LengthCol, typeof(int)),
            new(CostRangeCol, typeof(string)),
            new(NeighborsCol, typeof(Neighborhoods)),
            new(SmoothCol, typeof(SmoothLevels)),
            new(ObstaclesCol, typeof(int)),
            new(StatusCol, typeof(GraphStatuses)),
        ]);
        table.PrimaryKey = [table.Columns[IdCol]];
        Table = table;
        SetTableStyle();
        int line = 1;
        if (Style.ShowHorizontalHeaderOverline)
        {
            line++;
        }
        if (Style.ShowHorizontalHeaderUnderline)
        {
            line++;
        }
        headerLinesConsumed = line;
        MultiSelect = true;
        FullRowSelect = true;
        X = 0;
        Y = Pos.Percent(0);
        Width = Dim.Fill();
        Height = Dim.Percent(85);
    }

    private void SetTableStyle()
    {
        var columnStyles = new Dictionary<DataColumn, ColumnStyle>()
        {
            { Table.Columns[IdCol], new() { Visible = false } },
            { Table.Columns[NameCol], new() { MinWidth = 17, MaxWidth = 17, Alignment = TextAlignment.Left } },
            { Table.Columns[WidthCol], new() { Alignment = TextAlignment.Centered } },
            { Table.Columns[LengthCol], new() { Alignment = TextAlignment.Centered } },
            { Table.Columns[CostRangeCol], new() { Alignment = TextAlignment.Centered } },
            { Table.Columns[NeighborsCol], new() { Alignment = TextAlignment.Left,
                RepresentationGetter = NeighborhoodToString } },
            { Table.Columns[SmoothCol], new() { Alignment = TextAlignment.Left,
                RepresentationGetter = SmoothLevelToString } },
            { Table.Columns[ObstaclesCol], new() { Alignment = TextAlignment.Centered } },
            { Table.Columns[StatusCol], new () { Alignment = TextAlignment.Centered,
                RepresentationGetter = GraphStatusToString } },
        };
        Style = new TableStyle()
        {
            ExpandLastColumn = false,
            ShowVerticalCellLines = false,
            AlwaysShowHeaders = true,
            ShowVerticalHeaderLines = false,
            ColumnStyles = columnStyles
        };
    }

    private static string SmoothLevelToString(object level)
    {
        var lvl = (SmoothLevels)level;
        return lvl.ToStringRepresentation();
    }

    private static string NeighborhoodToString(object neighborhood)
    {
        var n = (Neighborhoods)neighborhood;
        return n.ToStringRepresentation();
    }

    private static string GraphStatusToString(object status)
    {
        var s = (GraphStatuses)status;
        return s.ToStringRepresentation();
    }
}
