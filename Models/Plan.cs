namespace Luna.Api.Models;

public class Plan
{
    public string? Id { get; set; }
    public string? PlanId { get; set; }
    public string? Month { get; set; }
    public string? Year { get; set; }
    public string? Type { get; set; }
    public decimal? Amount { get; set; }
    public string? User { get; set; }
    public DateOnly? PayDate { get; set; }
    public string? ExpenseDescription { get; set; }
    public DateOnly? ExpenseDueDate { get; set; }
    public bool ExpenseStatus { get; set; }
}