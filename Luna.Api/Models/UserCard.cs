namespace Luna.Api.Models;

public class UserCard
{
    public string PlanId { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string Month { get; set; }
    public string Year { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string CurrentBalance { get; set; }
    public string TotalIncome { get; set; }
    public string ExpensesPaid { get; set; }
    public string ExpensesUnpaid { get; set; }
    public string Surplus { get; set; }
    public string EndingBalance { get; set; }
}