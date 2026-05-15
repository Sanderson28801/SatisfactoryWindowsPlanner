using System.Text.Json.Serialization;

namespace Planner.Core.Models
{
    public record BuildingData
    {
        [JsonPropertyName("slug")]
        public required string Slug { get; init; }

        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("className")]
        public required string ClassName { get; init; }

        [JsonPropertyName("metadata")]
        public required BuildingMetadata Metadata { get; init; }
    }

    public record BuildingMetadata
    {
        [JsonPropertyName("powerConsumption")]
        public decimal PowerConsumption { get; init; }

        // We can capture these too in case you want to use overclocking math later!
        [JsonPropertyName("powerConsumptionExponent")]
        public decimal PowerConsumptionExponent { get; init; }

        [JsonPropertyName("manufacturingSpeed")]
        public decimal ManufacturingSpeed { get; init; }
    }
}