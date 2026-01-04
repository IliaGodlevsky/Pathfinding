using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Models;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Logging.Interface;
using ReactiveUI;
using System.Collections.Frozen;

namespace Pathfinding.App.Console.ViewModels;

[ViewModel]
internal abstract class ViewModel(ILog log) : ReactiveObject
{
    public readonly record struct ActiveGraph(int Id, Graph<GraphVertexModel> Graph, bool IsReadonly = false)
    {
        public static readonly ActiveGraph Empty = new(0, Graph<GraphVertexModel>.Empty, false);

        public IReadOnlyDictionary<long, GraphVertexModel> VertexMap { get; } = Graph.ToFrozenDictionary(x => x.Id);
    }

    protected readonly ILog log = log;

    protected async Task ExecuteSafe(
        Func<CancellationToken, Task> action,
        Action onError = null)
    {
        try
        {
            double seconds = Settings.Default.BaseTimeoutSeconds;
            TimeSpan timeout = TimeSpan.FromSeconds(seconds);
            using var cts = new CancellationTokenSource(timeout);
            await action(cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex)
        {
            log.Warn(ex, ex.Message);
            onError?.Invoke();
        }
        catch (Exception ex)
        {
            log.Error(ex, ex.Message);
            onError?.Invoke();
        }
    }
}
