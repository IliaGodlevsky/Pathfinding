using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Messages.View;
using Pathfinding.Presentation.Console.Models;
using Pathfinding.Presentation.Console.ViewModels.Interface;
using Pathfinding.Shared.Extensions;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Collections.Specialized;
using System.Data;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.Presentation.Console.Views;

internal sealed partial class RunsTableView : TableView
{
    private readonly Dictionary<int, IDisposable> modelsSubs = [];
    private readonly Dictionary<string, bool> sortOrder = [];
    private readonly CompositeDisposable disposables = [];
    private readonly IRunsTableViewModel viewModel;
    private string filter = string.Empty;
    private string sortExpression = string.Empty;

    public RunsTableView(IRunsTableViewModel viewModel,
        [KeyFilter(KeyFilters.Views)] IMessenger messenger) : this()
    {
        this.viewModel = viewModel;
        viewModel.Runs.CollectionChanged += OnCollectionChanged;
        this.Events().KeyPress
            .Do(args => messenger.Send(new KeyPressedMessage(args)))
            .Where(x => x.KeyEvent.Key.HasFlag(Key.A)
                && x.KeyEvent.Key.HasFlag(Key.CtrlMask)
                && Table.Rows.Count > 0)
            .Throttle(TimeSpan.FromMilliseconds(50))
            .Select(x => MultiSelectedRegions
                    .SelectMany(x => (x.Rect.Top, x.Rect.Bottom - 1).Iterate())
                    .Select(GetRunId)
                    .ToArray())
            .InvokeCommand(viewModel, x => x.SelectRunsCommand)
            .DisposeWith(disposables);
        this.Events().SelectedCellChanged
            .Where(x => x.NewRow > -1 && x.NewRow < Table.Rows.Count)
            .Select(x => x.NewRow)
            .DistinctUntilChanged()
            .Select(x => GetSelectedRows())
            .InvokeCommand(viewModel, x => x.SelectRunsCommand)
            .DisposeWith(disposables);
        this.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Select(x => x.MouseEvent.Y + RowOffset - headerLinesConsumed)
            .Where(x => x >= 0 && x < Table.Rows.Count && x == SelectedRow)
            .Select(x => GetRunId(x).Enumerate().ToArray())
            .InvokeCommand(viewModel, x => x.SelectRunsCommand)
            .DisposeWith(disposables);
        this.Events().KeyPress
            .Where(args => args.KeyEvent.Key.HasFlag(Key.R)
                && args.KeyEvent.Key.HasFlag(Key.CtrlMask)
                && Table.Rows.Count > 1)
            .Do(x => OrderTable(IdCol, Ascending))
            .Select(x => GetSelectedRows())
            .InvokeCommand(viewModel, x => x.SelectRunsCommand)
            .DisposeWith(disposables);
        this.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked
                && Table.Rows.Count > 1
                && x.MouseEvent.Y < headerLinesConsumed)
            .Do(OrderOnMouseClick)
            .Select(x => GetSelectedRows())
            .InvokeCommand(viewModel, x => x.SelectRunsCommand)
            .DisposeWith(disposables);
        this.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Do(x => messenger.Send(new OpenRunFieldMessage()))
            .Subscribe()
            .DisposeWith(disposables);
    }

    private int[] GetSelectedRows()
    {
        var selected = GetAllSelectedCells().Select(x => x.Y)
                .Distinct().Select(GetRunId).ToArray();
        return selected;
    }

    private IDisposable BindTo<T>(RunInfoModel model, string column,
        Expression<Func<RunInfoModel, T>> expression)
    {
        return model.WhenAnyValue(expression)
            .Do(x => Update(model.Id, column, x))
            .Subscribe();
    }

    private void Update<T>(int id, string column, T value)
    {
        Application.MainLoop.Invoke(() =>
        {
            var row = sourceTable.Rows.Find(id);
            if (row is not null)
            {
                row[column] = value;
                sourceTable.AcceptChanges();
                ApplyFilter(filter);
            }
        });
    }

    internal void ApplyFilter(string value)
    {
        filter = value?.Trim() ?? string.Empty;
        var filtered = sourceTable.Clone();
        var rows = sourceTable.AsEnumerable().Where(row =>
            string.IsNullOrEmpty(filter) ||
            row.ItemArray.Any(cell => cell?.ToString()?.Contains(
                filter, StringComparison.OrdinalIgnoreCase) == true) ||
            GetRenderedValues(row).Any(value => value?.Contains(
                filter, StringComparison.OrdinalIgnoreCase) == true));
        foreach (var row in rows)
        {
            filtered.ImportRow(row);
        }

        if (!string.IsNullOrEmpty(sortExpression))
        {
            filtered.DefaultView.Sort = sortExpression;
            filtered = filtered.DefaultView.ToTable();
        }
        Table = filtered;
        SetTableStyle();
        MultiSelectedRegions.Clear();
        SetNeedsDisplay();
        SetCursorInvisible();
    }

    private static IEnumerable<string> GetRenderedValues(DataRow row)
    {
        yield return AlgorithmToString(row[AlgorithmCol]);
        yield return RunStatusToString(row[StatusCol]);
        if (row[StepCol] != DBNull.Value)
        {
            yield return StepRulesToString(row[StepCol]);
        }
        if (row[LogicCol] != DBNull.Value)
        {
            yield return HeuristicsToString(row[LogicCol]);
        }
    }

    private int GetRunId(int selectedRow)
    {
        return (int)Table.Rows[selectedRow][IdCol];
    }

    private void OrderOnMouseClick(MouseEventArgs args)
    {
        var selectedColumn = ScreenToCell(args.MouseEvent.X,
            headerLinesConsumed);
        var column = Table.Columns[selectedColumn.Value.X].ColumnName;
        var toSort = !sortOrder.GetValueOrDefault(column, true);
        sortOrder[column] = toSort;
        string order = toSort ? Ascending : Descending;
        OrderTable(column, order);
    }

    private void OrderTable(string columnName, string order)
    {
        sortExpression = $"{columnName} {order}";
        ApplyFilter(filter);
    }

    private static object ToTableValue<T>(T? value)
        where T : struct => value == null ? DBNull.Value : value.Value;

    private void OnAdded(RunInfoModel model)
    {
        sourceTable.Rows.Add(model.Id,
            model.Algorithm,
            model.Visited,
            model.Steps,
            model.Cost, model.Elapsed,
            ToTableValue(model.StepRule),
            ToTableValue(model.Heuristics),
            ToTableValue(model.Weight),
            model.ResultStatus);
        var sub = new CompositeDisposable();
        BindTo(model, VisitedCol, x => x.Visited).DisposeWith(sub);
        BindTo(model, StepsCol, x => x.Steps).DisposeWith(sub);
        BindTo(model, ElapsedCol, x => x.Elapsed).DisposeWith(sub);
        BindTo(model, CostCol, x => x.Cost).DisposeWith(sub);
        BindTo(model, StatusCol, x => x.ResultStatus).DisposeWith(sub);
        modelsSubs.Add(model.Id, sub);
        sourceTable.AcceptChanges();
        ApplyFilter(filter);
    }

    private void OnRemoved(RunInfoModel model)
    {
        var visibleRow = Table.Rows.Find(model.Id);
        var index = visibleRow is null ? -1 : Table.Rows.IndexOf(visibleRow);
        var row = sourceTable.Rows.Find(model.Id);
        if (row != null)
        {
            row.Delete();
            modelsSubs[model.Id].Dispose();
            modelsSubs.Remove(model.Id);
            sourceTable.AcceptChanges();
            ApplyFilter(filter);
            if (Table.Rows.Count > 0 && index >= 0)
            {
                SelectedCellChangedEventArgs args = index == Table.Rows.Count
                    ? new(Table, 0, 0, index, index - 1)
                    : new(Table, 0, 0, index, index);
                OnSelectedCellChanged(args);
                SetSelection(0, args.NewRow, false);
            }
        }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        Application.MainLoop.Invoke(() =>
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    MultiSelectedRegions.Clear();
                    sourceTable.Clear();
                    sourceTable.AcceptChanges();
                    modelsSubs.Values.ForEach(x => x.Dispose());
                    modelsSubs.Clear();
                    sortOrder.Clear();
                    sortExpression = string.Empty;
                    ApplyFilter(filter);
                    break;
                case NotifyCollectionChangedAction.Add:
                    OnAdded((RunInfoModel)e.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnRemoved((RunInfoModel)e.OldItems[0]);
                    break;
            }
            SetNeedsDisplay();
            SetCursorInvisible();
        });
    }

    protected override void Dispose(bool disposing)
    {
        disposables.Dispose();
        foreach (var sub in modelsSubs.Values)
        {
            sub.Dispose();
        }
        base.Dispose(disposing);
    }

    private static void SetCursorInvisible()
    {
        Application.Driver.SetCursorVisibility(CursorVisibility.Invisible);
    }
}
