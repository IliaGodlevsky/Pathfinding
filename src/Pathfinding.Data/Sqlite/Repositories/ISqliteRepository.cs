namespace Pathfinding.Data.Sqlite.Repositories;

internal interface ISqliteRepository
{
    static abstract string TableCreationScript { get; }
}
