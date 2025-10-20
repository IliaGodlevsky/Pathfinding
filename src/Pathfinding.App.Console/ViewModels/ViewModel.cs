using Pathfinding.App.Console.Injection;
using Pathfinding.Logging.Interface;
using ReactiveUI;

namespace Pathfinding.App.Console.ViewModels;

[ViewModel]
internal abstract class ViewModel(ILog log) : ReactiveObject
{
    protected readonly ILog log = log;

    protected async Task ExecuteSafe(
        Func<CancellationToken, Task> action,
        Action onError = null)
    {
        try
        {
            using var cts = new CancellationTokenSource(GetTimeout());
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

    protected virtual TimeSpan GetTimeout()
    {
        return TimeSpan.FromSeconds(Settings.Default.BaseTimeoutSeconds);
    }

    protected static TimeSpan GetTimeout(int multiplicator)
    {
        double timeout = Settings.Default.BaseTimeoutSeconds * multiplicator;
        return TimeSpan.FromSeconds(timeout);
    }
}
