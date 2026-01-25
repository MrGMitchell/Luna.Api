using Luna.Api.Models;
using Microsoft.Azure.Cosmos;

namespace Luna.Api.Services.Football;

public class FootballQuestionService : IFootballQuestionService
{
    private readonly CosmosClient _cosmosClient;
    private readonly string _footballQuestionsDatabaseId;
    private readonly string _footballQuestionsContainerId;

    public FootballQuestionService(IConfiguration configuration, CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
        _footballQuestionsDatabaseId = configuration["FootballQuestionsDatabaseId"]!;
        _footballQuestionsContainerId = configuration["FootballQuestionsContainerId"]!;
    }

    public async Task<FootballQuestion> GetDailyFootballQuestionAsync()
    {
        Container container = _cosmosClient.GetContainer(_footballQuestionsDatabaseId, _footballQuestionsContainerId);

        using FeedIterator<FootballQuestion> questionfeed = container.GetItemQueryIterator<FootballQuestion>(
            queryText: $"SELECT * FROM Questions q WHERE q.LastSent < DateTimeAdd(\"dd\",-30,GetCurrentDateTime())"
        );

        var question = new FootballQuestion();

        while (questionfeed.HasMoreResults)
        {
            FeedResponse<FootballQuestion> response = await questionfeed.ReadNextAsync();

            Random rnd = new Random();

            int r = rnd.Next(response.Count);

            question = response.ToList<FootballQuestion>()[r];
        }

        question.LastSent = DateTime.Now;

        await container.UpsertItemAsync<FootballQuestion>(question, new PartitionKey(question.QuestionId));

        return question;
    }

    public async Task<List<FootballQuestion>> GetQuizFootballQuestionsAsync(int numberOfQuestions)
    {
        Container container = _cosmosClient.GetContainer(_footballQuestionsDatabaseId, _footballQuestionsContainerId);

        using FeedIterator<FootballQuestion> questionfeed = container.GetItemQueryIterator<FootballQuestion>(
            queryText: $"SELECT * FROM Questions q"
        );

        var questions = new List<FootballQuestion>();

        while (questionfeed.HasMoreResults)
        {
            FeedResponse<FootballQuestion> response = await questionfeed.ReadNextAsync();

            Random rnd = new Random();

            while (numberOfQuestions-- > 0)
            {
                questions.Add(response.ToList<FootballQuestion>()[rnd.Next(response.Count)]);
            }
        }

        return questions;
    }

    public async Task<FootballQuestion> GetTodaysFootballQuestionAsync()
    {
        Container container = _cosmosClient.GetContainer(_footballQuestionsDatabaseId, _footballQuestionsContainerId);

        using FeedIterator<FootballQuestion> questionfeed = container.GetItemQueryIterator<FootballQuestion>(
            queryText: $"SELECT Top 1 * FROM c Order By c.LastSent DESC"
        );

        var question = new FootballQuestion();

        while (questionfeed.HasMoreResults)
        {
            FeedResponse<FootballQuestion> response = await questionfeed.ReadNextAsync();

            question = response.First<FootballQuestion>();
        }

        return question;
    }

    public async Task<List<QuizQuestion>> GetQuizQuestionsAsync()
    {
        Container container = _cosmosClient.GetContainer(_footballQuestionsDatabaseId, _footballQuestionsContainerId);

        using FeedIterator<QuizQuestion> questionfeed = container.GetItemQueryIterator<QuizQuestion>(
            queryText: $"SELECT * FROM Questions q Where q.CorrectAnswer <> ''"
        );

        var questions = new List<QuizQuestion>();

        var numberOfQuestions = 10;

        var questionsUsed = new List<int>();

        while (questionfeed.HasMoreResults)
        {
            FeedResponse<QuizQuestion> response = await questionfeed.ReadNextAsync();

            Random rnd = new();

            while (numberOfQuestions-- > 0)
            {
                var questionIndex = rnd.Next(response.Count);

                while (questionsUsed.Contains(questionIndex))
                {
                    questionIndex = rnd.Next(response.Count);
                }

                questionsUsed.Add(questionIndex);

                questions.Add(response.ToList<QuizQuestion>()[questionIndex]);
            }
        }

        return questions;
    }
}