using LearnApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnApi.Controllers;

public class CommentController : ControllerBase
{
    private readonly TodoContext _context;
    private readonly IConfiguration _configuration; 
    public CommentController(TodoContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
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
        return Ok(comment);
    }

    [HttpGet]
    public async Task<IActionResult> All()
    {
        return Ok(await _context.Comments.ToListAsync());
    }
    
    [HttpPut]
    [Route("{id}")]
    public async Task<IActionResult> update(long id, string text, long profileId)
    {
        var comment = await _context.Comments.SingleOrDefaultAsync(c => c.Id == id && c.ProfileId == profileId);
        if (comment == null)
        {
            return BadRequest($"Comment doesn't exist! id: {id}");
        }

        _context.Entry(comment).CurrentValues.SetValues(new {Text = text, Id = id});
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> delete(long id)
    {
        var comment = await _context.Comments.SingleOrDefaultAsync(c => c.Id == id);
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
    public async Task<IActionResult> Dislike(long commentId, long profileId)
    {
        var like = await _context.CommentLikes.Include(c => c.Comment).SingleOrDefaultAsync(c => c.CommentId == commentId && c.ProfileId == profileId);
        if (like == null)
        {
            return BadRequest($"You can only dislike things you liked!");
        }
        try
        {
            _context.CommentLikes.Remove(like);
            _context.Entry(like.Comment).CurrentValues.SetValues(new {Likes = like.Comment.Likes - 1});
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
    public async Task<IActionResult> Like(long commentId, long profileId)
    {
        var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.Id == profileId);
        var comment = await _context.Comments.SingleOrDefaultAsync(c => c.Id == commentId);
        if (profile == null || comment == null)
        {
            return BadRequest($"Comment or profile doesn't exist! profile: {profileId} comment: {commentId}");
        }
        var commentLike = new CommentLike
        {
            Profile = profile,
            Comment = comment
        };

        try
        {
            var result = await _context.CommentLikes.AddAsync(commentLike);
            _context.Entry(comment).CurrentValues.SetValues(new {Likes = comment.Likes + 1});
            await _context.SaveChangesAsync();
            return Ok(result.Entity);
        }
        catch (Exception e)
        {
            return BadRequest("You can only like comments once!");
        }
    }
}