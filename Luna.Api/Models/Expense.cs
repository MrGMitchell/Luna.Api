namespace Luna.Api.Models;

public class Expense
{
    public string? Id { get; set; }
    public string? PlanId { get; set; }
    public string? Type { get; set; }
    public string? User { get; set; }
    public string? ExpenseDescription { get; set; }
    public string? Amount { get; set; }
    public string? ExpenseDueDate { get; set; }
    public bool ExpenseStatus { get; set; }
}