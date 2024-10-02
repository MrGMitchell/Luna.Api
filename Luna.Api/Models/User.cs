namespace Luna.Api.Models;

public class User
{
    public string UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string CurrentBalance { get; set; }
    public string Paid { get; set; }
    public string Unpaid { get; set; }
    public string Surplus { get; set; }
    public string EndingBalance { get; set; }
}