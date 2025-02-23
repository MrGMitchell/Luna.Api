using System.Net;
using Luna.Api.Models;
using Luna.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Luna.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ExpenseController : ControllerBase
{
    private readonly ILogger<ExpenseController> _logger;
    private readonly ICosmosDbService _cosmosDb;

    public ExpenseController(ILogger<ExpenseController> logger, ICosmosDbService cosmosDb)
    {
        _logger = logger;
        _cosmosDb = cosmosDb;
    }
    
    [HttpPost(Name = "CreateUserExpense")]
    public async Task<HttpStatusCode> AddUserExpense(Expense expense)
    {
        return await _cosmosDb.CreateUserExpenseAsync(expense);
    }
}