using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.Service.Interface.Models.Serialization.Csv
{
    internal class CsvGraph
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public SmoothLevels SmoothLevel { get; set; }

        public Neighborhoods Neighborhood { get; set; }

        public GraphStatuses Status { get; set; }

        public string DimensionSizes { get; set; }
    }
}
