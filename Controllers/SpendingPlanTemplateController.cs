using System.Net;
using Luna.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Luna.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SpendingPlanTemplateController : ControllerBase
{
    private readonly ILogger<SpendingPlanTemplateController> _logger;
    private readonly ICosmosDbService _cosmosDb;

    public SpendingPlanTemplateController(ILogger<SpendingPlanTemplateController> logger, ICosmosDbService cosmosDb)
    {
        _logger = logger;
        _cosmosDb = cosmosDb;
    }
    
    [HttpPost(Name = "CreateSpendingPlanTemplate")]
    public async Task<HttpStatusCode> CreateSpendingPlanTemplate()
    {
        return await _cosmosDb.CreateSpendingPlanTemplateAsync();
    }
}