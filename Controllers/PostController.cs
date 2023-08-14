using System.Security.Claims;
using LearnApi.Models;
using LearnApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LearnApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;

    public PostController(
        IPostService postService,
        ICommentService commentService
        )
    {
        _postService = postService;
        _commentService = commentService;
    }

    [HttpGet]
    [Route("feed")]
    public async Task<IActionResult> Feed(int pageSize = 10, int pageNumber = 1, string order = "date")
    {
        var sessionId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var response = await _postService.GetAll(pageNumber, pageSize);

        if (response.Success)
        {
            return Ok(new
            {
                currentPage = response.CurrentPage,
                lastPage = response.LastPage,
                posts = response.Data
            });            
        }

        Console.WriteLine("Hello");
        return BadRequest(response.Error);
        Console.WriteLine("ciao");
    }
    
    [HttpGet]
    [Route("{postId}/comments")]
    public async Task<IActionResult> GetComments(long postId, int pageSize = 10, int pageNumber = 1, string order = "likes")
    {
        var sessionId = HttpContext.Request.Cookies["Auth"];
        var response = await _commentService.GetByPostId(postId, pageSize, pageNumber);
        return BadRequest("Order can be either date or likes. Yours was: " + order);
    }
    
    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> getById(long id)
    {
        var response = await _postService.GetById(id);
        if (response.Success)
        {
            return Ok(response.Data);
        }
        else
        {
            return BadRequest(response.Error);
        }
    }
 
    [HttpPost]
    public async Task<IActionResult> create(CreatePostDto postDto)
    {
        var sessionId = HttpContext.Request.Cookies["Auth"];
        if (String.IsNullOrEmpty(sessionId))
        {
            return BadRequest("login to create post!");
        }
        var response = await _postService.Add(postDto, sessionId);
        if (response.Success)
        {
            return Ok(response.Data);
        }
        else
        {
            return BadRequest(response.Error);
        }
    }   
     
    [HttpPost]
    [Route("{postId}/comments")]
    public async Task<IActionResult> AddComment(long postId,CreateCommentDto commentPayload)
    {
        var sessionId = HttpContext.Request.Cookies["Auth"];
        if (String.IsNullOrEmpty(sessionId))
        {
            return BadRequest("log in!");
        }
        var response = await _postService.AddComment(postId, commentPayload, sessionId);
        if (response.Success)
        {
            return Ok(response.Data);
        }
        else
        {
            return BadRequest(response.Error);
        }
    }

    [HttpPost]
    [Route("{postId}/likes")]
    public async Task<IActionResult> Like(long postId)
    {
        var sessionId = HttpContext.Request.Cookies["Auth"];
        if (String.IsNullOrEmpty(sessionId))
        {
            return BadRequest("Log in !");
        }

        var response = await _postService.Like(postId, sessionId);
        if (response.Success)
        {
            return Ok();
        }
        else
        {
            return BadRequest();
        }
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<IActionResult> Edit(long id, CreatePostDto postDto)
    {
         var sessionId = HttpContext.Request.Cookies["Auth"];
         if (String.IsNullOrEmpty(sessionId))
         {
             return BadRequest("Log in !");
         }
 
         var response = await _postService.Update(id, postDto, sessionId);
         if (response.Success)
         {
             return Ok(response.Data);
         }
         else
         {
             return BadRequest(response.Error);
         }       
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> deleteById(long id)
    {
        var sessionId = HttpContext.Request.Cookies["Auth"];
        if (String.IsNullOrEmpty(sessionId))
        {
            return BadRequest("Log in !");
        }

        var response = await _postService.Delete(id, sessionId);
        if (response.Success)
        {
            return Ok();
        }
        else
        {
            return BadRequest();
        }
    }

    [HttpDelete]
    [Route("{postId}/likes")]
    public async Task<IActionResult> Dislike(long postId)
    {
        var sessionId = HttpContext.Request.Cookies["Auth"];
        if (String.IsNullOrEmpty(sessionId))
        {
            return BadRequest("Log in !");
        }

        var response = await _postService.Dislike(postId, sessionId);
        if (response.Success)
        {
            return Ok();
        }
        else
        {
            return BadRequest();
        }
    }
}