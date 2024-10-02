namespace Luna.Api.Models;

public class MonthlySpendingPlan
{
    public string PlanId { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string Month { get; set; }
    public string Year { get; set; }
}