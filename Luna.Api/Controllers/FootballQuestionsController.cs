using Microsoft.AspNetCore.Mvc;
using Luna.Api.Services;
using Luna.Api.Models;

namespace Luna.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class FootballQuestionController : ControllerBase
{
    private readonly ILogger<FootballQuestionController> _logger;
    private readonly ICosmosDbService _cosmosDb;

    public FootballQuestionController(ILogger<FootballQuestionController> logger, ICosmosDbService cosmosDb)
    {
        _logger = logger;
        _cosmosDb = cosmosDb;
    }

    [HttpGet(Name = "GetFootballQuestion")]
    public async Task<FootballQuestion> GetFootballQuestion()
    {
        return await _cosmosDb.GetFootballQuestionAsync();
    }
}
