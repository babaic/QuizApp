using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using QuizService.Model;
using System.Threading.Tasks;
using QuizService.Repositories;

namespace QuizService.Controllers;

//TODO changed from Controller to ControllerBase since these are WEB API methods
[ApiController]
[Route("api/quizzes")]
public class QuizController : ControllerBase
{
    private readonly IQuizRepository _quizRepository;

    public QuizController(IQuizRepository quizRepository)
    {
        _quizRepository = quizRepository;
    }

    // GET api/quizzes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<QuizResponseModel>>> Get()
    {
        var quizzes = await _quizRepository.GetAllAsync();
        return Ok(quizzes);
    }

    // GET api/quizzes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<QuizResponseModel>> Get(int id)
    {
        var quiz = await _quizRepository.GetByIdAsync(id);
        if (quiz is null)
        {
            return NotFound();
        }

        return Ok(quiz);
    }

    // POST api/quizzes
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] QuizCreateModel value)
    {
        //TODO add validation check before creating
        var id = await _quizRepository.CreateAsync(value.Title);
        //TODO Would return id instead of null
        return Created($"/api/quizzes/{id}", null);
    }

    // PUT api/quizzes/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] QuizUpdateModel value)
    {
        var success = await _quizRepository.UpdateAsync(id, value.Title);
        if (!success)
            return NotFound();
        
        return NoContent();
    }

    // DELETE api/quizzes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _quizRepository.DeleteAsync(id);
        if (!success)
            return NotFound();
        
        return NoContent();
    }

    // POST api/quizzes/5/questions
    [HttpPost]
    [Route("{id}/questions")]
    public async Task<IActionResult> PostQuestion(int id, [FromBody] QuestionCreateModel value)
    {
        var isQuizExisting = await _quizRepository.ExistsAsync(id);
        if (!isQuizExisting)
        {
            return NotFound();
        }
        
        var questionId = await _quizRepository.CreateQuestionAsync(id, value.Text);
        return Created($"/api/quizzes/{id}/questions/{questionId}", questionId);
    }
    
    [HttpPost]
    [Route("{id}/questions/{qid}/answer/{answerId}")]
    public async Task<IActionResult> AnswerQuestion(int id, int qid, int answerId)
    {
        try
        {
            await _quizRepository.AnswerQuestionAsync(id, qid, answerId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT api/quizzes/5/questions/6
    [HttpPut("{id}/questions/{qid}")]
    public async Task<IActionResult> PutQuestion(int id, int qid, [FromBody] QuestionUpdateModel value)
    {
        //TODO Unused id parameter
        var success = await _quizRepository.UpdateQuestionAsync(qid, value.Text, value.CorrectAnswerId);
        if (!success)
            return NotFound();
        
        return NoContent();
    }

    // DELETE api/quizzes/5/questions/6
    [HttpDelete]
    [Route("{id}/questions/{qid}")]
    public async Task<IActionResult> DeleteQuestion(int id, int qid)
    {
        var success = await _quizRepository.DeleteQuestionAsync(qid);
        if (!success)
            return NotFound();
        
        return NoContent();
    }

    // POST api/quizzes/5/questions/6/answers
    [HttpPost]
    [Route("{id}/questions/{qid}/answers")]
    public async Task<IActionResult> PostAnswer(int id, int qid, [FromBody] AnswerCreateModel value)
    {
        var answerId = await _quizRepository.CreateAnswerAsync(qid, value.Text);
        return Created($"/api/quizzes/{id}/questions/{qid}/answers/{answerId}", answerId);
    }

    // PUT api/quizzes/5/questions/6/answers/7
    [HttpPut("{id}/questions/{qid}/answers/{aid}")]
    public async Task<IActionResult> PutAnswer(int id, int qid, int aid, [FromBody] AnswerUpdateModel value)
    {
        //TODO unused id and quid parameters
        var success = await _quizRepository.UpdateAnswerAsync(aid, value.Text);
        if (!success)
            return NotFound();
        
        return NoContent();
    }

    // DELETE api/quizzes/5/questions/6/answers/7
    [HttpDelete]
    [Route("{id}/questions/{qid}/answers/{aid}")]
    public async Task<IActionResult> DeleteAnswer(int id, int qid, int aid)
    {
        var success = await _quizRepository.DeleteAnswerAsync(aid);
        if (!success)
            return NotFound();
        
        return NoContent();
    }
}