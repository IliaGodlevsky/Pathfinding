using Pathfinding.Logging.Interface;
using Serilog;
using Serilog.Events;

namespace Pathfinding.Logging.Loggers
{
    public sealed class FileLog : ILog, IDisposable
    {
        public FileLog()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path: "logs\\log-.txt",
                    rollingInterval: RollingInterval.Month,
                    restrictedToMinimumLevel: LogEventLevel.Information)
                .CreateLogger();
        }

        public void Debug(string message)
        {
            Log.Logger.Debug(message);
        }

        public void Error(Exception ex, string message = null)
        {
            Log.Logger.Error(ex, message ?? ex.Message);
        }

        public void Error(string message)
        {
            Log.Logger.Error(message);
        }

        public void Fatal(Exception ex, string message = null)
        {
            Log.Logger.Fatal(ex, message ?? ex.Message);
        }

        public void Fatal(string message)
        {
            Log.Logger.Fatal(message);
        }

        public void Info(string message)
        {
            Log.Logger.Information(message);
        }

        public void Trace(string message)
        {
            Log.Logger.Information(message);
        }

        public void Warn(Exception ex, string message = null)
        {
            Log.Logger.Warning(ex, message ?? ex.Message);
        }

        public void Warn(string message)
        {
            Log.Logger.Warning(message);
        }

        public void Dispose()
        {
            Log.CloseAndFlush();
        }
    }
}
