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
    
    [HttpPost("AddSubscriberAsync")]
    public async Task<IActionResult> AddSubscriberAsync([FromBody] Subscriber subscriber)
    {
        if (subscriber == null || string.IsNullOrWhiteSpace(subscriber.Email))
        {
            return BadRequest("Invalid subscriber data.");
        }

        await _cosmosDb.AddSubscriberAsync(subscriber);
        return Ok();
    }

    [HttpDelete("DeleteSubscriberAsync")]
    public async Task<IActionResult> DeleteSubscriberAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email cannot be empty.");
        }

        await _cosmosDb.DeleteSubscriberAsync(email);
        return Ok();
    }
}