using ReactiveUI;

namespace Pathfinding.App.Console.ViewModels
{
    internal abstract class BaseViewModel : ReactiveObject
    {
        protected static async Task ExecuteSafe(Func<Task> action, Action<Exception, string> log)
        {
            try
            {
                await action().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log(ex, ex.Message);
            }
        }
    }
}
