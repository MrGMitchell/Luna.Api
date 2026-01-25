using System.Net;
using Luna.Api.Models;
using Luna.Api.Services.CosmosDB;
using Microsoft.AspNetCore.Mvc;

namespace Luna.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class IncomeController : ControllerBase
{
    private readonly ILogger<IncomeController> _logger;
    private readonly ICosmosDbService _cosmosDb;

    public IncomeController(ILogger<IncomeController> logger, ICosmosDbService cosmosDb)
    {
        _logger = logger;
        _cosmosDb = cosmosDb;
    }
    
    [HttpPost("CreateUserIncome")]
    public async Task<HttpStatusCode> AddUserIncome(Income income)
    {
        return await _cosmosDb.CreateUserIncomeAsync(income);
    }

    [HttpPut("UpdateUserIncome")]
    public async Task<HttpStatusCode> UpdateUserIncome(List<Income> incomes)
    {
        return await _cosmosDb.UpdateUserIncomeAsync(incomes);
    }   
}