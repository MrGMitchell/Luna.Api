namespace Luna.Api.Models;

public class UserCard
{
    public string? PlanId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Month { get; set; }
    public string? Year { get; set; }
    public string? Name { get; set; }
    public decimal? CurrentBalance { get; set; }
    public decimal? TotalIncome { get; set; }
    public decimal? ExpensesPaid { get; set; }
    public decimal? ExpensesUnpaid { get; set; }
    public decimal? Surplus { get; set; }
    public decimal? EndingBalance { get; set; }
    public List<Plan>? Incomes { get; set; }
    public List<Plan>? Expenses { get; set; }
}