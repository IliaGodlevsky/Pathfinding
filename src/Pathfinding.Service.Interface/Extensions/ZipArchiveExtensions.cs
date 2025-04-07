using CsvHelper;
using Pathfinding.Service.Interface.Models.Serialization;
using Pathfinding.Service.Interface.Models.Serialization.Csv;
using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace Pathfinding.Service.Interface.Extensions
{
    internal static class ZipArchiveExtensions
    {
        private static List<T> ReadEntry<T>(this ZipArchive archive, string entryName)
        {
            var entry = archive.GetEntry(entryName);
            if (entry is not null)
            {
                using var entryStream = entry.Open();
                using var reader = new StreamReader(entryStream, 
                    Encoding.UTF8, leaveOpen: true, bufferSize: 1024);
                using var csvReader = new CsvReader(reader, 
                    CultureInfo.InvariantCulture, leaveOpen: true);
                return [.. csvReader.GetRecords<T>()];
            }
            return [];
        }

        private static void WriteEntry<T>(this ZipArchive archive, 
            string entryName, IEnumerable<T> records)
        {
            var toWrite = records.ToList();
            if (toWrite.Count > 0)
            {
                var entry = archive.CreateEntry(entryName);
                using var entryStream = entry.Open();
                using var writer = new StreamWriter(entryStream,
                    Encoding.UTF8, leaveOpen: true, bufferSize: 1024);
                using var csvWriter = new CsvWriter(writer,
                    CultureInfo.InvariantCulture, leaveOpen: true);
                csvWriter.WriteRecords(toWrite);
            }
        }

        public static void WriteHistory(this ZipArchive archive,
            IEnumerable<PathfindingHistorySerializationModel> model)
        {
            var records = model.ToHistory();
            archive.WriteEntry("graphs.csv", records.Select(x => x.Graph));
            archive.WriteEntry("vertices.csv", records.SelectMany(x => x.Vertices));
            archive.WriteEntry("statistics.csv", records.SelectMany(x => x.Statistics));
            archive.WriteEntry("ranges.csv", records.SelectMany(x => x.Range));
        }

        public static List<PathfindingHistorySerializationModel> ReadHistory(this ZipArchive archive)
        {
            var graphs = archive.ReadEntry<CsvGraph>("graphs.csv");
            var vertices = archive.ReadEntry<CsvVertex>("vertices.csv");
            var statistics = archive.ReadEntry<CsvStatistics>("statistics.csv");
            var ranges = archive.ReadEntry<CsvRange>("ranges.csv");
            return (graphs, vertices, statistics, ranges).ToHistory();
        }
    }
}
