namespace Pathfinding.Logging.Interface;

public interface ILog
{
    void Trace(string message);

    void Warn(Exception ex, string message = null);

    void Warn(string message);

    void Error(Exception ex, string message = null);

    void Error(string message);

    void Fatal(Exception ex, string message = null);

    void Fatal(string message);

    void Info(string message);

    void Debug(string message);
}