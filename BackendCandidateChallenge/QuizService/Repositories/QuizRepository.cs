using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using QuizService.Model;
using QuizService.Model.Domain;

namespace QuizService.Repositories;

public interface IQuizRepository
{
    Task<IEnumerable<QuizResponseModel>> GetAllAsync();
    Task<QuizResponseModel> GetByIdAsync(int id);
    Task<int> CreateAsync(string title);
    Task<bool> UpdateAsync(int id, string title);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    
    // Question methods
    Task<int> CreateQuestionAsync(int quizId, string text);
    Task<bool> UpdateQuestionAsync(int questionId, string text, int correctAnswerId);
    Task<bool> DeleteQuestionAsync(int questionId);
    
    // Answer methods
    Task<int> CreateAnswerAsync(int questionId, string text);
    Task<bool> UpdateAnswerAsync(int answerId, string text);
    Task<bool> DeleteAnswerAsync(int answerId);
    Task AnswerQuestionAsync(int quizId, int questionId, int answerId);//TODO Added this method
}

public class QuizRepository : IQuizRepository
{
    private readonly IDbConnection _connection;

    public QuizRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<QuizResponseModel>> GetAllAsync()
    {
        const string sql = "SELECT * FROM Quiz;";
        var quizzes = await _connection.QueryAsync<Quiz>(sql);
        return quizzes.Select(quiz => 
            new QuizResponseModel 
            { 
                Id = quiz.Id, 
                Title = quiz.Title 
            });
    }

    public async Task<QuizResponseModel> GetByIdAsync(int id)
    {
        const string quizSql = "SELECT * FROM Quiz WHERE Id = @Id;";
        var quiz = await _connection.QuerySingleOrDefaultAsync<Quiz>(quizSql, new { Id = id });
        
        if (quiz == null)
            return null;
        
        const string questionsSql = "SELECT * FROM Question WHERE QuizId = @QuizId;";
        var questions = await _connection.QueryAsync<Question>(questionsSql, new { QuizId = id });
        
        const string answersSql = "SELECT a.Id, a.Text, a.QuestionId FROM Answer a INNER JOIN Question q ON a.QuestionId = q.Id WHERE q.QuizId = @QuizId;";
        var answers = await _connection.QueryAsync<Answer>(answersSql, new { QuizId = id });
        
        const string userAnswerSql = "SELECT QuestionId, AnswerId, Score FROM QuizResponse WHERE QuizId = @QuizId AND UserId = @UserId;";
        var userResponses = await _connection.QueryAsync<(int QuestionId, int AnswerId, int? Score)>(userAnswerSql, new { QuizId = id, UserId = 1 });
        var userAnswers = userResponses.ToDictionary(
            r => r.QuestionId,
            r => new { r.AnswerId, r.Score }
        );
        
        var answersGrouped = answers
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return new QuizResponseModel
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Questions = questions.Select(question => new QuizResponseModel.QuestionItem
            {
                Id = question.Id,
                Text = question.Text,
                Answers = answersGrouped.ContainsKey(question.Id)
                    ? answersGrouped[question.Id].Select(answer => new QuizResponseModel.AnswerItem
                    {
                        Id = answer.Id,
                        Text = answer.Text
                    })
                    : new QuizResponseModel.AnswerItem[0],
                CorrectAnswerId = question.CorrectAnswerId,
                UserAnswerId = userAnswers.ContainsKey(question.Id) ? userAnswers[question.Id].AnswerId : null,
                Score = userAnswers.ContainsKey(question.Id) ? userAnswers[question.Id].Score : null,
            }),
            Links = new Dictionary<string, string>
            {
                {"self", $"/api/quizzes/{id}"},
                {"questions", $"/api/quizzes/{id}/questions"}
            }
        };
    }
    
    public async Task<int> CreateAsync(string title)
    {
        const string sql = "INSERT INTO Quiz (Title) VALUES(@Title); SELECT LAST_INSERT_ROWID();";
        return await _connection.ExecuteScalarAsync<int>(sql, new { Title = title });
    }

    public async Task<bool> UpdateAsync(int id, string title)
    {
        const string sql = "UPDATE Quiz SET Title = @Title WHERE Id = @Id";
        int rowsUpdated = await _connection.ExecuteAsync(sql, new { Id = id, Title = title });
        return rowsUpdated > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Quiz WHERE Id = @Id";
        int rowsDeleted = await _connection.ExecuteAsync(sql, new { Id = id });
        return rowsDeleted > 0;
    }
    
    public async Task<int> CreateQuestionAsync(int quizId, string text)
    {
        const string sql = "INSERT INTO Question (Text, QuizId) VALUES(@Text, @QuizId); SELECT LAST_INSERT_ROWID();";
        return await _connection.ExecuteScalarAsync<int>(sql, new { Text = text, QuizId = quizId });
    }

    public async Task<bool> UpdateQuestionAsync(int questionId, string text, int correctAnswerId)
    {
        const string sql = "UPDATE Question SET Text = @Text, CorrectAnswerId = @CorrectAnswerId WHERE Id = @QuestionId";
        int rowsUpdated = await _connection.ExecuteAsync(sql, new { QuestionId = questionId, Text = text, CorrectAnswerId = correctAnswerId });
        return rowsUpdated > 0;
    }

    public async Task<bool> DeleteQuestionAsync(int questionId)
    {
        const string sql = "DELETE FROM Question WHERE Id = @QuestionId";
        int rowsDeleted = await _connection.ExecuteAsync(sql, new { QuestionId = questionId });
        return rowsDeleted > 0;
    }

    public async Task<int> CreateAnswerAsync(int questionId, string text)
    {
        const string sql = "INSERT INTO Answer (Text, QuestionId) VALUES(@Text, @QuestionId); SELECT LAST_INSERT_ROWID();";
        return await _connection.ExecuteScalarAsync<int>(sql, new { Text = text, QuestionId = questionId });
    }

    public async Task<bool> UpdateAnswerAsync(int answerId, string text)
    {
        const string sql = "UPDATE Answer SET Text = @Text WHERE Id = @AnswerId";
        int rowsUpdated = await _connection.ExecuteAsync(sql, new { AnswerId = answerId, Text = text });
        return rowsUpdated > 0;
    }

    public async Task<bool> DeleteAnswerAsync(int answerId)
    {
        const string sql = "DELETE FROM Answer WHERE Id = @AnswerId";
        int rowsDeleted = await _connection.ExecuteAsync(sql, new { AnswerId = answerId });
        return rowsDeleted > 0;
    }

    public async Task AnswerQuestionAsync(int quizId, int questionId, int answerId)
    {
        const string questionSql = "SELECT Id, CorrectAnswerId FROM Question WHERE Id = @QuestionId AND QuizId = @QuizId;";
        var question = await _connection.QuerySingleOrDefaultAsync<Question>(questionSql, new { QuestionId = questionId, QuizId = quizId });
        
        if (question == null)
            throw new KeyNotFoundException($"Question with id {questionId} not found");
        
        const string answerSql = "SELECT COUNT(1) FROM Answer WHERE Id = @AnswerId AND QuestionId = @QuestionId;";
        var answerExists = await _connection.ExecuteScalarAsync<int>(answerSql, new { AnswerId = answerId, QuestionId = questionId });
        
        if (answerExists == 0)
            throw new KeyNotFoundException($"Answer with id {answerId} not found");
        
        const string insertSql = @"INSERT INTO QuizResponse (QuizId, QuestionId, AnswerId, UserId, Score) 
                                   VALUES(@QuizId, @QuestionId, @AnswerId, @UserId, @Score);";
        await _connection.ExecuteAsync(insertSql, new 
        { 
            QuizId = quizId,
            QuestionId = questionId,
            AnswerId = answerId,
            UserId = 1, //TODO hardcoded myself
            Score = answerId == question.CorrectAnswerId ? 1 : 0,
        });
    }

    public async Task<bool> ExistsAsync(int id)
    {
        const string sql = "SELECT COUNT(1) FROM Quiz WHERE Id = @Id";
        var count = await _connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        return count > 0;
    }
}