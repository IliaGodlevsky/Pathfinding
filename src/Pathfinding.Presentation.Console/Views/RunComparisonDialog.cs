using Pathfinding.Domain.Enums;
using Pathfinding.Presentation.Console.Extensions;
using Pathfinding.Presentation.Console.Models;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed class RunComparisonDialog : Dialog
{
    private readonly Button closeButton = new("Close");

    public RunComparisonDialog(IReadOnlyCollection<RunInfoModel> selectedRuns)
    {
        Title = $"Compare {selectedRuns.Count} runs";
        Width = 66;
        Height = 13;

        var successful = selectedRuns
            .Where(run => run.ResultStatus == RunStatuses.Success)
            .ToArray();
        if (successful.Length == 0)
        {
            Add(new Label("None of the selected runs found a path.")
            {
                X = 1,
                Y = 1
            });
        }
        else
        {
            AddResult(1, "Fastest", successful.MinBy(run => run.Elapsed),
                run => $"{run.Elapsed.TotalMilliseconds:F2} ms");
            AddResult(3, "Lowest cost", successful.MinBy(run => run.Cost),
                run => $"{run.Cost:F2}");
            AddResult(5, "Fewest steps", successful.MinBy(run => run.Steps),
                run => run.Steps.ToString());
            AddResult(7, "Fewest visited", successful.MinBy(run => run.Visited),
                run => run.Visited.ToString());
        }

        closeButton.Clicked += OnClose;
        AddButton(closeButton);
    }

    private void AddResult(int row, string metric, RunInfoModel run,
        Func<RunInfoModel, string> formatValue)
    {
        Add(new Label($"{metric,-15} {run.Algorithm.ToStringRepresentation(),-18} " +
            $"run #{run.Id,-6} {formatValue(run)}")
        {
            X = 1,
            Y = row,
            Width = Dim.Fill(1)
        });
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
