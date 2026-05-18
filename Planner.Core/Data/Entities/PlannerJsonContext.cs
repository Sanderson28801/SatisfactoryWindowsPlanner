using Planner.Core.Data.Entities;
using System.Text.Json.Serialization;

namespace Planner.Core.Models
{
    // The [JsonSerializable] attribute tells the compiler: 
    // "Please generate high-speed parsing code for the SatisfactoryData class"
    [JsonSerializable(typeof(SatisfactoryData))]
    public partial class PlannerJsonContext : JsonSerializerContext
    {
    }
}