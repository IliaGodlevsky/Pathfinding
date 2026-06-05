using Pathfinding.Data;
using Pathfinding.Logging.Interface;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Models;
using ReactiveUI;
using System.Collections.Frozen;

namespace Pathfinding.Presentation.Console.ViewModels;

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
            int multiplier = 1;
#if DEBUG
            multiplier = 600;
#endif
            double seconds = Settings.Default.BaseTimeoutSeconds * multiplier;
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
