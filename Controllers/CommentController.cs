using System.Security.Claims;
using LearnApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly TodoContext _context;
    private readonly IConfiguration _configuration;
    private readonly UserManager<Profile> _userManager;

    public CommentController(TodoContext context, IConfiguration configuration, UserManager<Profile> userManager)
    {
        _context = context;
        _configuration = configuration;
        _userManager = userManager;
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> ById(long id)
    {
        var comment = await _context.Comments.Include(c => c.Profile).SingleOrDefaultAsync(c => c.Id == id);
        if (comment == null)
        {
            return BadRequest($"Comment with this id: {id} doesn't exist!");
        }
        return Ok(new CommentDto(comment));
    }

    [HttpGet]
    public async Task<IActionResult> All()
    {
        return Ok(await _context.Comments.Select(c => new CommentDto(c)).ToListAsync());
    }
    
    [HttpPut]
    [Route("{id}")]
    public async Task<IActionResult> update(long id, string text)
    {
        var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        var comment = await _context.Comments.SingleOrDefaultAsync(c => c.Id == id && c.Profile.UserName == username);
        if (comment == null)
        {
            return BadRequest($"Comment doesn't exist! id: {id}");
        }

        comment.Text = text;
        await _context.SaveChangesAsync();
        return Ok(comment);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> delete(long id)
    {
        var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        var comment = await _context.Comments
            .Include(c => c.Profile)
            .Include(c => c.Post)
            .SingleOrDefaultAsync(c => c.Id == id && c.Profile.UserName == username);
        if (comment == null)
        {
            return BadRequest($"Comment doesn't exist! id: {id}");
        }
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete]
    [Route("{commentId}/likes")]
    public async Task<IActionResult> Dislike(long commentId)
    {
        var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        var like = await _context.CommentLikes.Include(c => c.Comment).SingleOrDefaultAsync(c => c.CommentId == commentId && c.Profile.UserName == username);
        if (like == null)
        {
            return BadRequest($"You can only dislike things you liked!");
        }
        try
        {
            _context.CommentLikes.Remove(like);
            like.Comment.Likes = like.Comment.Likes - 1;
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest("Something went wrong when removing the comment...");
        }
    }
    
    [HttpPost]
    [Route("{commentId}/likes")]
    public async Task<IActionResult> Like(long commentId)
    {  
        Console.WriteLine("cookie: ", HttpContext.Request.Cookies.Count);
        var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.UserName == username);
        var comment = await _context.Comments.SingleOrDefaultAsync(c => c.Id == commentId);
        if (profile == null || comment == null)
        {
            return BadRequest($"Comment or profile doesn't exist! profile: {username} comment: {commentId}");
        }
        var commentLike = new CommentLike
        {
            Profile = profile,
            Comment = comment,
            CreatedAt = DateTime.Now
        };

        try
        {
            var result = await _context.CommentLikes.AddAsync(commentLike);
            comment.Likes = comment.Likes + 1;
            await _context.SaveChangesAsync();
            return Ok(result.Entity);
        }
        catch (Exception e)
        {
            return BadRequest("You can only like comments once!");
        }
    }
}