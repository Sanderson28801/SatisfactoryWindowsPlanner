namespace Planner.Api.Models;

public class EdgeDto
{
    public required string Source { get; set; }
    public required string Target { get; set; }
    public decimal Amount { get; set; }
}
