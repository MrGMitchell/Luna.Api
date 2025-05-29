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

    [HttpGet("GetDailyFootballQuestion")]
    public async Task<FootballQuestion> GetDailyFootballQuestion()
    {
        return await _cosmosDb.GetDailyFootballQuestionAsync();
    }

    [HttpGet("GetQuizFootballQuestions")]
    public async Task<List<FootballQuestion>> GetQuizFootballQuestions(int numberOfQuestions)
    {
        return await _cosmosDb.GetQuizFootballQuestionsAsync(numberOfQuestions);
    }

    [HttpGet("GetTodaysFootballQuestionAsync")]
    public async Task<FootballQuestion> GetTodaysFootballQuestionAsync()
    {
        return await _cosmosDb.GetTodaysFootballQuestionAsync();
    }

    [HttpGet("GetSubscribersAsync")]
    public async Task<List<Subscriber>> GetSubscribersAsync()
    {
        return await _cosmosDb.GetSubscribersAsync();
    }
}