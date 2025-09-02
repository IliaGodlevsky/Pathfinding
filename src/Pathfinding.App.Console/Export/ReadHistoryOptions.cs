using Autofac.Features.Metadata;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Models;
using Pathfinding.Service.Interface.Models.Serialization;

namespace Pathfinding.App.Console.Export;

internal sealed class ReadHistoryOptions : IReadHistoryOptions
{
    private readonly Dictionary<ExportOptions, IReadHistoryOption> options;

    public IReadOnlyList<ExportOptions> Allowed { get; }

    public ReadHistoryOptions(Meta<IReadHistoryOption>[] options)
    {
        this.options = options.ToDictionary(
            x => (ExportOptions)x.Metadata[MetadataKeys.ExportOptions], 
            x => x.Value);
        Allowed = [.. this.options.Keys];
    }

    public async Task<PathfindingHistoriesSerializationModel> ReadHistoryAsync(
        ExportOptions option, 
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        if (options.TryGetValue(option, out var value))
        {
            return await value.ReadHistoryAsync(graphIds, token).ConfigureAwait(false);
        }

        throw new KeyNotFoundException($"{option} was not found");
    }
}