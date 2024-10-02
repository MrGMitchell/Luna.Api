namespace Luna.Api.Models;

public class Expense
{
    public string ExpenseId { get; set; }
    public string UserId { get; set; }
    public string Description { get; set; }
    public string Amount { get; set; }
    public string DueDate { get; set; }
    public string CheckNumber { get; set; }    
    public string Paid { get; set; }
    public string Notes { get; set; }
}