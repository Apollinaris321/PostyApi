using System.Security.Claims;
using LearnApi.Models;
using LearnApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearnApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostController : ControllerBase
{
    private readonly TodoContext _context;
    private readonly IConfiguration _configuration;
    private readonly UserManager<Profile> _userManager;

    public PostController(TodoContext context, IConfiguration configuration, UserManager<Profile> userManager)
    {
        _context = context;
        _configuration = configuration;
        _userManager = userManager;
    }

    // /posts -> post, get
    // /posts/id -> get, delete, put
    // /posts/id/likes -> post, delete, get
    // /posts/id/comments -> post, get
    [HttpGet]
    [Route("{postId}/comments")]
    public async Task<IActionResult> GetComments(long postId)
    {
        var comments = await _context.Comments
            .Include(comment => comment.Profile)
            .Where(comment => comment.PostId == postId)
            .Select(c => new CommentDto(c))
            .ToListAsync();
        return Ok(comments);
    }

    [HttpPost]
    [Route("{postId}/comments")]
    public async Task<IActionResult> AddComment(long postId, string text)
    {
        var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.UserName == username);
        var post = await _context.Posts.SingleOrDefaultAsync(post => post.Id == postId);
        if (profile == null || post == null)
        {
            return BadRequest("Couldnt find profile or post");
        }

        var newComment = new Comment
        {
            Post = post,
            Profile = profile,
            Text = text,
            CreatedAt = DateTime.Now,
            Likes = 0,
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
        var likes = await _context.PostLikes.Include(like => like.Profile).Where(like => like.PostId == postId)
            .Select(l => new { Username = l.Profile.UserName }).ToListAsync();
        return Ok(likes);
    }

    [HttpDelete]
    [Route("{postId}/likes")]
    public async Task<IActionResult> Dislike(long postId)
    {
        var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        var like = await _context.PostLikes.Include(like => like.Post)
            .SingleOrDefaultAsync(like => like.Profile.UserName == username && like.PostId == postId);
        if (like == null)
        {
            return BadRequest($"You can only dislike things you liked before!");
        }

        _context.PostLikes.Remove(like);
        like.Post.Likes = like.Post.Likes - 1;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost]
    [Route("{postId}/likes")]
    public async Task<IActionResult> Like(long postId)
    {
        var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.UserName == username);
        var post = await _context.Posts.SingleOrDefaultAsync(post => post.Id == postId);
        if (post == null || profile == null)
        {
            return BadRequest($"Post with id: {postId} doesn't exist! User {username} doesn't exist");
        }

        var newLike = new PostLike
        {
            Profile = profile,
            Post = post,
        };

        try
        {
            await _context.PostLikes.AddAsync(newLike);
            post.Likes = post.Likes + 1;
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
    public async Task<IActionResult> Edit(long id, CreatePostDto postDto)
    {
        var post = await _context.Posts.SingleOrDefaultAsync(p => p.Id == id);
        if (post == null)
        {
            return BadRequest($"Post with id: {id} doesn't exist!");
        }

        post.Text = postDto.Text;
        post.Title = postDto.Title;
        _context.Entry(post).CurrentValues.SetValues(new { Text = postDto.Text, Title = post.Title });
        await _context.SaveChangesAsync();
        return Ok(post);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> deleteById(long id)
    {
        var post = await _context.Posts
            .Include(p => p.Profile)
            .Include(p => p.Comments)
            .SingleOrDefaultAsync(p => p.Id == id);
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
        var post = await _context.Posts.Include(post => post.Profile).Select(p => new PostDto(p))
            .SingleOrDefaultAsync(p => p.Id == id);
        if (post == null)
        {
            return BadRequest($"Post with id: {id} doesn't exist!");
        }

        return Ok(post);
    }

    [HttpPost]
    public async Task<IActionResult> create(CreatePostDto postDto)
    {
        var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        if (username == null)
        {
            return BadRequest("Can't create posts for other users!");
        }

        var owner = await _context.Profiles.SingleOrDefaultAsync(p => p.UserName == username);
        if (owner == null)
        {
            return BadRequest($"Could not find your profile!");
        }

        var newPost = new Post
        {
            Profile = owner,
            Title = postDto.Title,
            Text = postDto.Text,
            CreatedAt = DateTime.Now,
            Likes = 0
        };

        var result = await _context.Posts.AddAsync(newPost);
        await _context.SaveChangesAsync();
        return Ok(result.Entity);
    }

    [HttpGet]
    public async Task<IActionResult> getAll([FromQuery] PaginationFilter filter)
    {
        var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
        var response = await _context.Posts
            .Include(post => post.Profile)
            .Select(post => new PostDto(post))
            .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
            .Take(validFilter.PageSize)
            .ToListAsync();

        string url = HttpContext.Request.Path;
        return Ok(new {res = response, url = url });
}
         
         
}