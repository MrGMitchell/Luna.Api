namespace Luna.Api.Models;

public class Expense
{
    public string? id { get; set; }
    public string? PlanId { get; set; }
    public string? Type { get; set; }
    public string? User { get; set; }
    public string? ExpenseDescription { get; set; }
    public decimal? Amount { get; set; }
    public DateOnly? ExpenseDueDate { get; set; }
    public bool ExpenseStatus { get; set; }
}