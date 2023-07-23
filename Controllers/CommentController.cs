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
        var comment = await _context.Comments
            .Include(c => c.Profile)
            .SingleOrDefaultAsync(c => c.Id == id);
        
        if (comment == null)
        {
            return BadRequest($"Comment with this id: {id} doesn't exist!");
        }
        
        return Ok(new CommentDto(comment));
    }

    [HttpGet]
    public async Task<IActionResult> All()
    {
        return Ok(await _context.Comments
            .Select(c => new CommentDto(c))
            .ToListAsync());
    }
    
    [HttpPost]
    [Route("{commentId}/likes")]
    public async Task<IActionResult> Like(long commentId)
    {  
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
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest("You can only like comments once!");
        }
    }
     
    [HttpPut]
    [Route("{id}")]
    public async Task<IActionResult> update(long id, CreateCommentDto commentDto)
    {
        var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        var comment = await _context.Comments
            .Include(c => c.Profile)
            .SingleOrDefaultAsync(c => c.Id == id && c.Profile.UserName == username);
        if (comment == null)
        {
            return BadRequest($"Comment doesn't exist or doesn't belong to you! id: {id}");
        }

        comment.Text = commentDto.Text;
        await _context.SaveChangesAsync();
        return Ok(new CommentDto(comment));
    }
 
    [HttpDelete]
    [Route("{commentId}/likes")]
    public async Task<IActionResult> Dislike(long commentId)
    {
        var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        var like = await _context.CommentLikes
            .Include(c => c.Comment)
            .Include(p => p.Profile)
            .SingleOrDefaultAsync(c => c.CommentId == commentId && c.Profile.UserName == username);
        if (like == null)
        {
            return BadRequest($"You can only dislike things you liked!");
        }
        try
        {
            _context.CommentLikes.Remove(like);
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest("Something went wrong when removing the like...");
        }
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
            return BadRequest($"Comment doesn't exist or doesn't belong to you! id: {id}");
        }
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return Ok();
    }
   
}