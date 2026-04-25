using Microsoft.AspNetCore.Mvc;
using Luna.Api.Services.CosmosDB;
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

    [HttpGet("GetQuizQuestions")]
    public async Task<List<QuizQuestion>> GetQuizQuestions()
    {
        return await _cosmosDb.GetQuizQuestionsAsync();
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

    [HttpPost("SaveQuizAnswersAsync")]
    public async Task<IActionResult> SaveQuizAnswersAsync([FromBody] QuizAnswer quizAnswer)
    {
        if (quizAnswer == null || string.IsNullOrWhiteSpace(quizAnswer.id) || string.IsNullOrWhiteSpace(quizAnswer.UserId))
        {
            return BadRequest("Invalid quiz answer data. Id and UserId are required.");
        }

        if (quizAnswer.Answers == null || quizAnswer.Answers.Count == 0)
        {
            return BadRequest("Quiz answer must contain at least one answer.");
        }

        var statusCode = await _cosmosDb.SaveQuizAnswersAsync(quizAnswer);
        
        return statusCode == System.Net.HttpStatusCode.OK || statusCode == System.Net.HttpStatusCode.Created
            ? Ok($"Quiz answers saved successfully.")
            : StatusCode((int)statusCode, "Failed to save quiz answers.");
    }

    [HttpGet("user/{userId}/summary")]
    public async Task<IActionResult> GetUserQuizSummary(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("UserId cannot be empty.");
        }

        try
        {
            var summary = await _cosmosDb.GetUserQuizSummaryAsync(userId);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving quiz summary for user {userId}: {ex.Message}");
            return StatusCode(500, "An error occurred while retrieving quiz summary.");
        }
    }

    [HttpGet("user/{userId}/history")]
    public async Task<IActionResult> GetUserQuizHistory(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("UserId cannot be empty.");
        }

        try
        {
            var history = await _cosmosDb.GetUserQuizHistoryAsync(userId);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving quiz history for user {userId}: {ex.Message}");
            return StatusCode(500, "An error occurred while retrieving quiz history.");
        }
    }
}