using Pathfinding.App.Console.Extensions;
using Pathfinding.Domain.Core.Enums;
using System.Data;
using System.Globalization;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views
{
    internal sealed partial class RunsTableView : TableView
    {
        private const string IdCol = "Id";
        private const string AlgorithmCol = "Algorithm";
        private const string VisitedCol = "Visited";
        private const string StepsCol = "Steps";
        private const string CostCol = "Cost";
        private const string ElapsedCol = "Elapsed";
        private const string StepCol = "Step";
        private const string LogicCol = "Logic";
        private const string WeightCol = "Weight";
        private const string StatusCol = "Status";

        private const string Ascending = "ASC";
        private const string Descending = "DESC";

        private readonly int headerLinesConsumed;

        private void SetTableStyle()
        {
            Table.PrimaryKey = [Table.Columns[IdCol]];
            var columnStyles = new Dictionary<DataColumn, ColumnStyle>()
            {
                { Table.Columns[IdCol], new() { Visible = false } },
                { Table.Columns[AlgorithmCol], new() { MinWidth = 12, MaxWidth = 12, Alignment = TextAlignment.Left,
                    RepresentationGetter = AlgorithmToString } },
                { Table.Columns[VisitedCol], new() { Alignment = TextAlignment.Centered } },
                { Table.Columns[StepsCol], new() { Alignment = TextAlignment.Centered } },
                { Table.Columns[CostCol], new() { Alignment = TextAlignment.Centered } },
                { Table.Columns[ElapsedCol], new() { Alignment = TextAlignment.Centered,
                    RepresentationGetter = TimeToMilliseconds} },
                { Table.Columns[StepCol], new() { MinWidth = 9, MaxWidth = 9, Alignment = TextAlignment.Centered,
                    RepresentationGetter = StepRulesToString } },
                { Table.Columns[LogicCol], new() { MinWidth = 9, MaxWidth = 9, Alignment = TextAlignment.Centered,
                    RepresentationGetter = HeuristicsToString } },
                { Table.Columns[WeightCol], new() { Alignment = TextAlignment.Left } },
                { Table.Columns[StatusCol], new() { Alignment = TextAlignment.Centered,
                    RepresentationGetter = RunStatusToString } }
            };
            Style = new()
            {
                ExpandLastColumn = true,
                ShowVerticalCellLines = false,
                AlwaysShowHeaders = true,
                SmoothHorizontalScrolling = true,
                ShowHorizontalHeaderOverline = true,
                ShowVerticalHeaderLines = false,
                ColumnStyles = columnStyles,
                ShowHorizontalScrollIndicators = true
            };
        }

        private static string TimeToMilliseconds(object time)
        {
            var t = (TimeSpan)time;
            return Math.Round(t.TotalMilliseconds, 2).ToString(CultureInfo.InvariantCulture);
        }

        private static string AlgorithmToString(object algorithm)
        {
            var a = (Algorithms)algorithm;
            return a.ToStringRepresentation();
        }

        private static string StepRulesToString(object stepRule)
        {
            var stepRules = (StepRules?)stepRule;
            return stepRules?.ToStringRepresentation();
        }

        private static string HeuristicsToString(object heuristic)
        {
            var heuristics = (HeuristicFunctions?)heuristic;
            return heuristics?.ToStringRepresentation();
        }

        private static string RunStatusToString(object status)
        {
            var s = (RunStatuses)status;
            return s.ToStringRepresentation();
        }

        public RunsTableView()
        {
            Table = new();
            Table.Columns.AddRange(
            [
                new (IdCol, typeof(int)),
                new (AlgorithmCol, typeof(Algorithms)),
                new (VisitedCol, typeof(int)),
                new (StepsCol, typeof(int)),
                new (CostCol, typeof(double)),
                new (ElapsedCol, typeof(TimeSpan)),
                new (StepCol, typeof(object)),
                new (LogicCol, typeof(object)),
                new (WeightCol, typeof(object)),
                new (StatusCol, typeof(RunStatuses))
            ]);
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
            Height = Dim.Percent(90);
        }
    }
}
