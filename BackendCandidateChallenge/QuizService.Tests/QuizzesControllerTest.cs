using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using QuizService.Model;
using QuizService.Model.Domain;
using Xunit;

namespace QuizService.Tests;

public class QuizzesControllerTest
{
    const string QuizApiEndPoint = "/api/quizzes/";

    [Fact]
    public async Task PostNewQuizAddsQuiz()
    {
        var quiz = new QuizCreateModel("Test title");
        using var testHost = new TestServer(new WebHostBuilder()
            .UseStartup<Startup>());
        var client = testHost.CreateClient();
        var content = new StringContent(JsonConvert.SerializeObject(quiz));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}"),
            content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task AQuizExistGetReturnsQuiz()
    {
        using var testHost = new TestServer(new WebHostBuilder()
            .UseStartup<Startup>());
        var client = testHost.CreateClient();
        const long quizId = 1;
        var response = await client.GetAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);
        var quiz = JsonConvert.DeserializeObject<QuizResponseModel>(await response.Content.ReadAsStringAsync());
        Assert.Equal(quizId, quiz.Id);
        Assert.Equal("My first quiz", quiz.Title);
    }

    [Fact]
    public async Task AQuizDoesNotExistGetFails()
    {
        using var testHost = new TestServer(new WebHostBuilder()
            .UseStartup<Startup>());
        var client = testHost.CreateClient();
        const long quizId = 999;
        var response = await client.GetAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}"));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AQuizDoesNotExists_WhenPostingAQuestion_ReturnsNotFound()
    {
        const string QuizApiEndPoint = "/api/quizzes/999/questions";

        using var testHost = new TestServer(new WebHostBuilder()
            .UseStartup<Startup>());
        var client = testHost.CreateClient();
        const long quizId = 999;
        var question = new QuestionCreateModel("The answer to everything is what?");
        var content = new StringContent(JsonConvert.SerializeObject(question));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}"), content);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task AQuizShouldCalculateCorrectAnswers()
    {
        var quiz = new QuizCreateModel("Geography quiz");
        using var testHost = new TestServer(new WebHostBuilder()
            .UseStartup<Startup>());
        var client = testHost.CreateClient();
        var content = new StringContent(JsonConvert.SerializeObject(quiz));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}"),
            content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        
        var quizId = int.Parse(response.Headers.Location.OriginalString.Split('/').Last());
        
        var questions = new List<Question>
        {
            new() { Text = "What is the capital of France?" },
            new() { Text = "Which country has the most population?" },
        };

        foreach (var ques in questions)
        {
            var question = new QuestionCreateModel(ques.Text);
            var questionContent = new StringContent(JsonConvert.SerializeObject(question));
            questionContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var questionResponse = await client.PostAsync(
                new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}/questions"), 
                questionContent);
            Assert.Equal(HttpStatusCode.Created, questionResponse.StatusCode);
            ques.Id = await questionResponse.Content.ReadFromJsonAsync<int>();
        }
        
        //Answers for question 1:
        var q1a1 = new AnswerCreateModel("Paris");
        var q1a1Content = new StringContent(JsonConvert.SerializeObject(q1a1));
        q1a1Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var q1a1Response = await client.PostAsync(
            new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}/questions/{questions[0].Id}/answers"),
            q1a1Content);
        var q1a1Id = await q1a1Response.Content.ReadFromJsonAsync<int>();
        
        var q1a2 = new AnswerCreateModel("Lyon");
        var q1a2Content = new StringContent(JsonConvert.SerializeObject(q1a2));
        q1a2Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var q1a2Response = await client.PostAsync(
            new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}/questions/{questions[0].Id}/answers"),
            q1a2Content);
        var q1a2Id = await q1a2Response.Content.ReadFromJsonAsync<int>();
        
        // Set correct answer for question 1
        var updateQ1 = new QuestionUpdateModel { Text = "What is the capital of France?", CorrectAnswerId = q1a1Id };
        var updateQ1Content = new StringContent(JsonConvert.SerializeObject(updateQ1));
        updateQ1Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        await client.PutAsync(
            new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}/questions/{questions[0].Id}"),
            updateQ1Content);
        
        // Answers for question 2:
        var q2a1 = new AnswerCreateModel("United States");
        var q2a1Content = new StringContent(JsonConvert.SerializeObject(q2a1));
        q2a1Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var q2a1Response = await client.PostAsync(
            new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}/questions/{questions[1].Id}/answers"),
            q2a1Content);
        var q2a1Id = await q2a1Response.Content.ReadFromJsonAsync<int>();
        
        var q2a2 = new AnswerCreateModel("India");
        var q2a2Content = new StringContent(JsonConvert.SerializeObject(q2a2));
        q2a2Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var q2a2Response = await client.PostAsync(
            new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}/questions/{questions[1].Id}/answers"),
            q2a2Content);
        var q2a2Id = await q2a2Response.Content.ReadFromJsonAsync<int>();
        
        // Set correct answer for question 2
        var updateQ2 = new QuestionUpdateModel { Text = "Which country has the most population?", CorrectAnswerId = q2a2Id };
        var updateQ2Content = new StringContent(JsonConvert.SerializeObject(updateQ2));
        updateQ2Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        await client.PutAsync(
            new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}/questions/{questions[1].Id}"),
            updateQ2Content);
        
        // User take quiz
        // Question 1, Answer: Paris ✅ 1 Point!
        var answerQ1Response = await client.PostAsync(
            new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}/questions/{questions[0].Id}/answer/{q1a1Id}"),
            null);
        Assert.Equal(HttpStatusCode.OK, answerQ1Response.StatusCode);
        
        // Question 2, Answer: United States ❌ 0 Point
        var answerQ2Response = await client.PostAsync(
            new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}/questions/{questions[1].Id}/answer/{q2a1Id}"),
            null);
        Assert.Equal(HttpStatusCode.OK, answerQ2Response.StatusCode);
        
        //get quiz
        var getQuizResponse = await client.GetAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}"));
        Assert.Equal(HttpStatusCode.OK, getQuizResponse.StatusCode);
    
        var createdQuiz = JsonConvert.DeserializeObject<QuizResponseModel>(
            await getQuizResponse.Content.ReadAsStringAsync());

        var totalScore = createdQuiz.Questions.Sum(q => q.Score ?? 0);
        Assert.Equal(1, totalScore);
    }
    
}
    