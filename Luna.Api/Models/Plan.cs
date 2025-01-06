namespace Luna.Api.Models;

public class Plan
{
    public string? PlanId { get; set; }
    public string? Month { get; set; }
    public string? Year { get; set; }
    public string? Type { get; set; }
    public string? Amount { get; set; }
    public string? User { get; set; }
    public string? PayDate { get; set; }
    public string? ExpenseDescription { get; set; }
    public string? ExpenseDueDate { get; set; }
    public bool ExpenseStatus { get; set; }
}