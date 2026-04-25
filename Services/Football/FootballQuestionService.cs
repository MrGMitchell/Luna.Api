using Luna.Api.Models;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace Luna.Api.Services.Football;

public class FootballQuestionService : IFootballQuestionService
{
    private readonly CosmosClient _cosmosClient;
    private readonly string _footballQuestionsDatabaseId;
    private readonly string _footballQuestionsContainerId;
    private readonly string _userQuizsContainerId;

    public FootballQuestionService(IConfiguration configuration, CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
        _footballQuestionsDatabaseId = configuration["FootballQuestionsDatabaseId"]!;
        _footballQuestionsContainerId = configuration["FootballQuestionsContainerId"]!;
        _userQuizsContainerId = configuration["UserQuizsContainerId"]!;
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

    public async Task<HttpStatusCode> SaveQuizAnswersAsync(QuizAnswer quizAnswer)
    {
        try
        {
            Container container = _cosmosClient.GetContainer(_footballQuestionsDatabaseId, _userQuizsContainerId);
            
            var response = await container.CreateItemAsync(quizAnswer);

            return response.StatusCode;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error saving quiz answers: {ex.Message}", ex);
        }
    }

    public async Task<UserReportSummary> GetUserQuizSummaryAsync(string userId)
    {
        try
        {
            Container container = _cosmosClient.GetContainer(_footballQuestionsDatabaseId, _userQuizsContainerId);

            var queryDefinition = new QueryDefinition(
                "SELECT * FROM c WHERE c.userId = @userId ORDER BY c.completedAt DESC"
            ).WithParameter("@userId", userId);

            using FeedIterator<QuizAnswer> queryResultSetIterator = container.GetItemQueryIterator<QuizAnswer>(queryDefinition);

            var quizAnswers = new List<QuizAnswer>();
            
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<QuizAnswer> response = await queryResultSetIterator.ReadNextAsync();
                quizAnswers.AddRange(response);
            }

            // Calculate overall statistics
            int totalQuizzesCompleted = quizAnswers.Count;
            int totalQuestionsAnswered = 0;
            int totalCorrectAnswers = 0;
            var categoryStats = new Dictionary<string, (int total, int correct)>();

            // Process all quizzes
            var recentQuizzes = new List<QuizReport>();

            foreach (var quiz in quizAnswers)
            {
                if (quiz.Answers == null || quiz.Answers.Count == 0)
                    continue;

                int quizCorrectCount = 0;
                var quizCategoryStats = new Dictionary<string, (int total, int correct)>();

                // Process answers within this quiz
                foreach (var answer in quiz.Answers)
                {
                    totalQuestionsAnswered++;
                    if (answer.IsCorrect)
                    {
                        totalCorrectAnswers++;
                        quizCorrectCount++;
                    }

                    // Track category statistics
                    if (answer.Categories != null)
                    {
                        foreach (var category in answer.Categories)
                        {
                            if (!categoryStats.ContainsKey(category))
                                categoryStats[category] = (0, 0);

                            var stats = categoryStats[category];
                            stats.total++;
                            if (answer.IsCorrect)
                                stats.correct++;
                            categoryStats[category] = stats;

                            if (!quizCategoryStats.ContainsKey(category))
                                quizCategoryStats[category] = (0, 0);

                            var quizStats = quizCategoryStats[category];
                            quizStats.total++;
                            if (answer.IsCorrect)
                                quizStats.correct++;
                            quizCategoryStats[category] = quizStats;
                        }
                    }
                }

                // Build category stats for this quiz
                var quizCategoryBreakdown = quizCategoryStats.Select(kvp => new CategoryStats
                {
                    Category = kvp.Key,
                    TotalQuestions = kvp.Value.total,
                    CorrectAnswers = kvp.Value.correct
                }).ToList();

                // Add to recent quizzes
                recentQuizzes.Add(new QuizReport
                {
                    QuizResultId = quiz.id,
                    CompletedAt = quiz.CompletedAt,
                    TotalQuestions = quiz.Answers.Count,
                    CorrectAnswers = quizCorrectCount,
                    CategoryStats = quizCategoryBreakdown
                });
            }

            // Build overall category breakdown
            var categoryBreakdown = categoryStats.Select(kvp => new CategoryStats
            {
                Category = kvp.Key,
                TotalQuestions = kvp.Value.total,
                CorrectAnswers = kvp.Value.correct
            }).OrderByDescending(c => c.TotalQuestions).ToList();

            // Calculate overall accuracy
            double overallAccuracy = totalQuestionsAnswered > 0 
                ? (totalCorrectAnswers * 100.0) / totalQuestionsAnswered 
                : 0;

            var summary = new UserReportSummary
            {
                TotalQuizzesTaken = totalQuizzesCompleted,
                OverallAccuracyPercentage = Math.Round(overallAccuracy, 2),
                TotalQuestionsAnswered = totalQuestionsAnswered,
                TotalCorrectAnswers = totalCorrectAnswers,
                CategoryBreakdown = categoryBreakdown,
                RecentQuizzes = recentQuizzes.Take(10).ToList() // Return top 10 recent quizzes
            };

            return summary;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving quiz summary for user {userId}: {ex.Message}", ex);
        }
    }

    public async Task<List<QuizAnswer>> GetUserQuizHistoryAsync(string userId)
    {
        try
        {
            Container container = _cosmosClient.GetContainer(_footballQuestionsDatabaseId, _userQuizsContainerId);

            var queryDefinition = new QueryDefinition(
                "SELECT * FROM c WHERE c.userId = @userId ORDER BY c.completedAt DESC"
            ).WithParameter("@userId", userId);

            using FeedIterator<QuizAnswer> queryResultSetIterator = container.GetItemQueryIterator<QuizAnswer>(queryDefinition);

            var quizAnswers = new List<QuizAnswer>();
            
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<QuizAnswer> response = await queryResultSetIterator.ReadNextAsync();
                quizAnswers.AddRange(response);
            }

            return quizAnswers;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving quiz history for user {userId}: {ex.Message}", ex);
        }
    }
}