using Pathfinding.Logging.Interface;
using ReactiveUI;

namespace Pathfinding.App.Console.ViewModels;

internal abstract class BaseViewModel(ILog log) : ReactiveObject
{
    protected static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);
    protected readonly ILog log = log;

    protected async Task ExecuteSafe(
        Func<Task> action,
        Action onError = null)
    {
        try
        {
            await action().ConfigureAwait(false);
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
