namespace Luna.Api.Models;

public class Balance
{
    public string? id { get; set; }
    public decimal? Amount { get; set; }
    public string? Type { get; set; }
    public string? User { get; set; }
    public string? PlanId { get; set; }
}