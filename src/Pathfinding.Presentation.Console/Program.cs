using Pathfinding.Presentation.Console.Injection;

await using (Modules
    .Initiate()
    .AddSqlite()
    .AddLogging()
    .AddGraphLayers()
    .AddDataTransfering()
    .AddPathfindingAlgorithms()
    .AddPathfindingRangeCommands()
    .AddTransitPathfindingRangeCommands()
    .BuildApplication()
    .RunApplication())
{
}