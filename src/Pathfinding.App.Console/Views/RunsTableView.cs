using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Shared.Extensions;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class RunsTableView : TableView
{
    private readonly Dictionary<int, IDisposable> modelsSubs = [];
    private readonly Dictionary<string, bool> sortOrder = [];
    private readonly CompositeDisposable disposables = [];

    public RunsTableView(IRunsTableViewModel viewModel,
        [KeyFilter(KeyFilters.Views)] IMessenger messenger) : this()
    {
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
            var row = Table.Rows.Find(id);
            row[column] = value;
            Table.AcceptChanges();
            SetNeedsDisplay();
            SetCursorInvisible();
        });
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
        Table.DefaultView.Sort = $"{columnName} {order}";
        Table = Table.DefaultView.ToTable();
        SetTableStyle();
        Table.AcceptChanges();
        SetNeedsDisplay();
    }

    private static object ToTableValue<T>(T? value)
        where T : struct => value == null ? DBNull.Value : value.Value;

    private void OnAdded(RunInfoModel model)
    {
        Table.Rows.Add(model.Id,
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
        Table.AcceptChanges();
    }

    private void OnRemoved(RunInfoModel model)
    {
        var row = Table.Rows.Find(model.Id);
        var index = Table.Rows.IndexOf(row);
        if (row != null)
        {
            row.Delete();
            modelsSubs[model.Id].Dispose();
            modelsSubs.Remove(model.Id);
            Table.AcceptChanges();
            MultiSelectedRegions.Clear();
            if (Table.Rows.Count > 0)
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
                    Table.Clear();
                    Table.AcceptChanges();
                    modelsSubs.Values.ForEach(x => x.Dispose());
                    modelsSubs.Clear();
                    sortOrder.Clear();
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
