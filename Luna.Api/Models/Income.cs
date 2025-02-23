namespace Luna.Api.Models;

public class Income
{
    public string? id { get; set; }
    public string? PlanId { get; set; }
    public string? Type { get; set; }
    public string? User { get; set; }
    public DateOnly? PayDate { get; set; }
    public decimal? Amount { get; set; }
}