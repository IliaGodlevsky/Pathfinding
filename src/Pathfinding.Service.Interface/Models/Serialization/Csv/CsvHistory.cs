namespace Pathfinding.Service.Interface.Models.Serialization.Csv
{
    internal class CsvHistory
    {
        public CsvGraph Graph { get; set; }

        public IReadOnlyCollection<CsvVertex> Vertices { get; set; }

        public IReadOnlyCollection<CsvStatistics> Statistics { get; set; }

        public IReadOnlyCollection<CsvRange> Range { get; set; }
    }
}
