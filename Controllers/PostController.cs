using LearnApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostController :ControllerBase
{
    private readonly TodoContext _context;
    private readonly IConfiguration _configuration;
 
    public PostController(TodoContext context,IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    
    // /posts -> post, get
    // /posts/id -> get, delete, put
    // /posts/id/likes -> post, delete, get
    // /posts/id/comments -> post, get
    [HttpGet]
    [Route("{postId}/comments")]
    public async Task<IActionResult> GetComments(long postId)
    {
        var comments = await _context.Comments.Include(comment => comment.Profile).Where(comment => comment.PostId == postId).ToListAsync();
        return Ok(comments);
    }
    
    [HttpPost]
    [Route("{postId}/comments")]
    public async Task<IActionResult> AddComment(long postId,string text, long profileId)
    {
        var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.Id == profileId);
        var post = await _context.Posts.SingleOrDefaultAsync(post => post.Id == postId);
        if (profile == null || post == null)
        {
            return BadRequest("Couldnt find profile or post");
        }
        var newComment = new Comment
        {
            Post = post,
            Profile = profile,
            Text = text
        };
        var result = _context.Comments.Add(newComment);
        post.Comments.Add(result.Entity);
        profile.Comments.Add(result.Entity);
        await _context.SaveChangesAsync();
        return Ok(result.Entity);
    }
       
    [HttpGet]
    [Route("{postId}/likes")]
    public async Task<IActionResult> GetLikes(long postId)
    {
        var likes = await _context.PostLikes.Include(like => like.Profile).Where(like => like.PostId == postId).ToListAsync();
        return Ok(likes);
    }            
    
    [HttpDelete]
    [Route("{postId}/likes")]
    public async Task<IActionResult> Dislike(long postId,long profileId)
    {
        var like = await _context.PostLikes.Include(like => like.Post).SingleOrDefaultAsync(like => like.ProfileId == profileId && like.PostId == postId);
        if (like == null)
        {
            return BadRequest($"Postlike with id: {postId} doesn't exist! User id : {profileId} doesn't exist");
        }

        _context.PostLikes.Remove(like);
        _context.Entry(like.Post).CurrentValues.SetValues(new {Likes = like.Post.Likes - 1});
        await _context.SaveChangesAsync();
        return Ok();
    }       
    
    [HttpPost]
    [Route("{postId}/likes")]
    public async Task<IActionResult> Like(long postId,long profileId)
    {
        // var existingLike =
        //     await _context.Likes.SingleOrDefaultAsync(l => l.ProfileId == profileId && l.PostId == postId);
        // if (existingLike != null)
        // {
        //     return BadRequest("Already liked this post!");
        // }
        var profile = await _context.Profiles.SingleOrDefaultAsync(profile => profile.Id == profileId);
        var post = await _context.Posts.SingleOrDefaultAsync(post => post.Id == postId);
        if (post == null || profile == null)
        {
            return BadRequest($"Post with id: {postId} doesn't exist! User id : {profileId} doesn't exist");
        }

        var newLike = new PostLike
        {
            Profile = profile,
            Post = post,
        };

        try
        {
            await _context.PostLikes.AddAsync(newLike);
            _context.Entry(post).CurrentValues.SetValues(new {Likes = post.Likes + 1});
            await _context.SaveChangesAsync();
            return Ok(post);           
        }
        catch (Exception e)
        {
            return BadRequest("Already liked this post!");
        }
    }   
    
    [HttpPut]
    [Route("{id}")]
    public async Task<IActionResult> Edit(long id, PostDto postDto)
    { 
        var post = await _context.Posts.SingleOrDefaultAsync(p => p.Id == id);
        if (post == null)
        {
            return BadRequest($"Post with id: {id} doesn't exist!");
        }
        
        _context.Entry(post).CurrentValues.SetValues(new {Text = postDto.Text});
        await _context.SaveChangesAsync();
        return Ok(post);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> deleteById(long id)
    { 
        var post = await _context.Posts.SingleOrDefaultAsync(p => p.Id == id);
        if (post == null)
        {
            return BadRequest($"Post with id: {id} doesn't exist!");
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> getById(long id)
    {
        var post = await _context.Posts.Include(post => post.Profile).SingleOrDefaultAsync(p => p.Id == id);
        if (post == null)
        {
            return BadRequest($"Post with id: {id} doesn't exist!");
        }
        return Ok(post);
    }

    [HttpPost]
    public async Task<IActionResult> create(PostDto post)
    {
        var owner = await _context.Profiles.SingleOrDefaultAsync(p => p.Id == post.ProfileId);
        if (owner == null)
        {
            return BadRequest($"Profile with id: {post.ProfileId} doesn't exist!");
        }

        var newPost = new Post
        {
            Profile = owner,
            Text = post.Text
        };

        var result = await _context.Posts.AddAsync(newPost);
        await _context.SaveChangesAsync();
        return Ok(result.Entity);
    }

    [HttpGet]
    public async Task<IActionResult> getAll()
    {
        return Ok(await _context.Posts.Include(post => post.Profile).ToListAsync());
    }
         
         
}