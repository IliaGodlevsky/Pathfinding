using Autofac.Features.AttributeFilters;
using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Messages.View;
using Pathfinding.App.Console.Models;
using Pathfinding.App.Console.ViewModels.Interface;
using Pathfinding.Shared.Extensions;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Terminal.Gui;

namespace Pathfinding.App.Console.Views;

internal sealed partial class GraphsTableView
{
    private readonly Dictionary<int, IDisposable> modelChangingSubs = [];

    public GraphsTableView(IGraphTableViewModel viewModel,
        [KeyFilter(KeyFilters.Views)] IMessenger messenger) : this()
    {
        viewModel.Graphs.ActOnEveryObject(AddToTable, RemoveFromTable);
        this.Events().Initialized
            .Select(_ => Unit.Default)
            .InvokeCommand(viewModel, x => x.LoadGraphsCommand);
        this.Events().CellActivated
            .Where(x => x.Row < table.Rows.Count)
            .Select(x => GetGraphId(x.Row))
            .InvokeCommand(viewModel, x => x.ActivateGraphCommand);
        this.Events().KeyPress
            .Where(x => x.KeyEvent.Key.HasFlag(Key.A)
                && x.KeyEvent.Key.HasFlag(Key.CtrlMask))
            .Throttle(TimeSpan.FromMilliseconds(150))
            .Select(_ => MultiSelectedRegions
                    .SelectMany(x => (x.Rect.Top, x.Rect.Bottom - 1).Iterate())
                    .Select(GetGraphId)
                    .ToArray())
            .InvokeCommand(viewModel, x => x.SelectGraphsCommand);
        this.Events().SelectedCellChanged
            .Where(x => x.NewRow > -1 && x.NewRow < table.Rows.Count)
            .Select(_ => GetAllSelectedCells().Select(x => x.Y)
                .Distinct().Select(GetGraphId).ToArray())
            .InvokeCommand(viewModel, x => x.SelectGraphsCommand);
        this.Events().MouseClick
            .Where(x => x.MouseEvent.Flags == MouseFlags.Button1Clicked)
            .Do(_ => messenger.Send(new CloseRunFieldMessage()))
            .Select(x => x.MouseEvent.Y + RowOffset - headerLinesConsumed)
            .Where(x => x >= 0 && x < Table.Rows.Count && x == SelectedRow)
            .Select(x => GetGraphId(x).Enumerate().ToArray())
            .InvokeCommand(viewModel, x => x.SelectGraphsCommand);
    }

    private int GetGraphId(int selectedRow)
    {
        return (int)Table.Rows[selectedRow][IdCol];
    }

    private void AddToTable(GraphInfoModel model)
    {
        Application.MainLoop.Invoke(() =>
        {
            table.Rows.Add(model.GetProperties());
            table.AcceptChanges();
            SetNeedsDisplay();
            var composite = new CompositeDisposable();
            BindTo(model, ObstaclesCol, x => x.ObstaclesCount).DisposeWith(composite);
            BindTo(model, NameCol, x => x.Name).DisposeWith(composite);
            BindTo(model, StatusCol, x => x.Status).DisposeWith(composite);
            BindTo(model, SmoothCol, x => x.SmoothLevel).DisposeWith(composite);
            BindTo(model, NeighborsCol, x => x.Neighborhood).DisposeWith(composite);
            modelChangingSubs.Add(model.Id, composite);
            SetCursorInvisible();
        });
    }

    private IDisposable BindTo<T>(GraphInfoModel model, string column,
        Expression<Func<GraphInfoModel, T>> expression)
    {
        return model.WhenAnyValue(expression)
            .Do(x => Update(model.Id, column, x))
            .Subscribe();
    }

    private void Update<T>(int id, string column, T value)
    {
        Application.MainLoop.Invoke(() =>
        {
            var row = table.Rows.Find(id);
            if (row is not null)
            {
                row[column] = value;
                table.AcceptChanges();
                SetNeedsDisplay();
                SetCursorInvisible();
            }
        });
    }

    private void RemoveFromTable(GraphInfoModel model)
    {
        Application.MainLoop.Invoke(() =>
        {
            var row = table.Rows.Find(model.Id);
            if (row is not null)
            {
                var index = table.Rows.IndexOf(row);
                row.Delete();
                table.AcceptChanges();
                modelChangingSubs[model.Id].Dispose();
                modelChangingSubs.Remove(model.Id);
                MultiSelectedRegions.Clear();
                if (table.Rows.Count > 0)
                {
                    SelectedCellChangedEventArgs args = index == table.Rows.Count
                        ? new(table, 0, 0, index, index - 1)
                        : new(table, 0, 0, index, index);
                    OnSelectedCellChanged(args);
                    SetSelection(0, args.NewRow, false);
                }
                SetNeedsDisplay();
                SetCursorInvisible();
            }
        });
    }

    private static void SetCursorInvisible()
    {
        Application.Driver.SetCursorVisibility(CursorVisibility.Invisible);
    }
}
