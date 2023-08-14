using LearnApi.Models;
using LearnApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearnApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentController(
        ICommentService commentService
        )
    {
        _commentService = commentService;
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> ById(long id)
    {
        var response = await _commentService.GetById(id);
        if (response.Success)
        {
            return Ok(response.Data);
        }
        return BadRequest(response.Error);
    }

    [HttpGet]
    public async Task<IActionResult> All(int pageNumber = 1 , int pageSize = 10)
    {
         var response = await _commentService.GetAll(pageNumber, pageSize);
         if (response.Success)
         {
             return Ok(response.Data);
         }
         return BadRequest(response.Error);       
    }
    
    [HttpPost]
    [Route("{commentId}/likes")]
    public async Task<IActionResult> Like(long commentId)
    {
        var sessionId = HttpContext.Request.Cookies["auth"];
        if (String.IsNullOrEmpty(sessionId))
        {
            return BadRequest("log in!");
        }
        var response = await _commentService.Like(commentId, sessionId);
        if (response.Success)
        {
            return Ok(response.Data);
        }
        return BadRequest(response.Error);
    }
     
    [HttpPut]
    [Route("{id}")]
    public async Task<IActionResult> update(long commentId, CreateCommentDto commentDto)
    {
        var sessionId = HttpContext.Request.Cookies["auth"];
        if (String.IsNullOrEmpty(sessionId))
        {
            return BadRequest("log in!");
        }
        var response = await _commentService.Update(commentDto, commentId,  sessionId);
        if (response.Success)
        {
            return Ok(response.Data);
        }
        return BadRequest(response.Error);
    }
    
    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> delete(long id)
    {
         var sessionId = HttpContext.Request.Cookies["auth"];
         if (String.IsNullOrEmpty(sessionId))
         {
             return BadRequest("log in!");
         }
         var response = await _commentService.Delete(id,  sessionId);
         if (response.Success)
         {
             return Ok(response.Data);
         }
         return BadRequest(response.Error);       
    }
}