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
    public async Task<IActionResult> GetComments(long postId, int pageSize = 10, int pageNumber = 1)
    {
        var len = _context.Comments
            .Count();
        var validFilter = new PaginationFilter(pageSize, len);
        validFilter.SetCurrentPage(pageNumber);
               
        var comments = await _context.Comments
            .Include(comment => comment.Profile)
            .Where(comment => comment.PostId == postId)
            .OrderByDescending(comment => comment.CreatedAt)
            .Select(c =>  new CommentDto(c))
            .Skip((validFilter.CurrentPage - 1) * validFilter.PageSize)
            .Take(validFilter.PageSize)
            .ToListAsync();

        return Ok(new { currentPage = validFilter.CurrentPage, lastPage = validFilter.LastPage, comments = comments });
    }

    [HttpPost]
    [Route("{postId}/comments")]
    public async Task<IActionResult> AddComment(long postId,CreateCommentDto commentPayload)
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
            Text = commentPayload.Text,
            CreatedAt = DateTime.Now,
            Likes = 0,
        };

        var result = _context.Comments.Add(newComment);
        post.Comments.Add(result.Entity);
        profile.Comments.Add(result.Entity);
        await _context.SaveChangesAsync();
        return Ok(new CommentDto(result.Entity));
    }

    [HttpGet]
    [Route("{postId}/likes")]
    public async Task<IActionResult> GetLikes(long postId)
    {
        var likes = await _context.PostLikes
            .Include(like => like.Profile)
            .Where(like => like.PostId == postId)
            .Select(l => new { Username = l.Profile.UserName})
            .ToListAsync();
        return Ok(likes);
    }

    [HttpDelete]
    [Route("{postId}/likes")]
    public async Task<IActionResult> Dislike(long postId)
    {
        var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        var like = await _context.PostLikes
            .Include(like => like.Post)
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
            return BadRequest("Already liked this post!" + e.Message);
        }
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<IActionResult> Edit(long id, CreatePostDto postDto)
    {
        var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        var post = await _context.Posts
            .Include(p => p.Profile)
            .SingleOrDefaultAsync(p => p.Id == id && p.Profile.UserName == username );
        if (post == null)
        {
            return BadRequest($"Post with id: {id} doesn't exist or it's not your post! {username} name");
        }

        try
        {
            post.Text = postDto.Text;
            post.Title = postDto.Title;
            _context.Entry(post).CurrentValues.SetValues(new { Text = postDto.Text, Title = post.Title });
            await _context.SaveChangesAsync();
            return Ok(post);
        }
        catch (Exception e)
        {
            return BadRequest("something went wrong when saving changes: " + e.Message);
        }
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> deleteById(long id)
    {
        var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        var post = await _context.Posts
            .Include(p => p.Profile)
            .Include(p => p.Comments)
            .SingleOrDefaultAsync(p => p.Id == id && p.Profile.UserName == username);
        if (post == null)
        {
            return BadRequest($"Post with id: {id} doesn't exist or you didn't own this post!");
        }

        try
        {

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest("something went wrong when saving changes: " + e.Message);
        }
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> getById(long id)
    {
        var post = await _context.Posts
            .Include(post => post.Profile)
            .SingleOrDefaultAsync(p => p.Id == id);
        if (post == null)
        {
            return BadRequest($"Post with id: {id} doesn't exist!");
        }

        return Ok(new PostDto(post));
    }

    [HttpPost]
    public async Task<IActionResult> create(CreatePostDto postDto)
    {
        var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        if (username == null)
        {
            return BadRequest("Register or Login to create posts!");
        }

        var owner = await _context.Profiles
            .SingleOrDefaultAsync(p => p.UserName == username);
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
        return Ok(new PostDto(result.Entity));
    }

    [HttpGet]
    [Route("feed")]
    public async Task<IActionResult> Feed(int pageSize = 10, int pageNumber = 1,string sort = "new")
    {
        if (sort != "new" && sort != "popular")
        {
            return BadRequest("Sort can either be new or popular!");
        }

        //if (sort == "new")
        var responseQuery = _context.Posts
            .Include(post => post.Profile)
            .OrderByDescending(p => p.CreatedAt)
            .Select(post => new PostDto(post));
        
        var len = responseQuery.Count();
        var validFilter = new PaginationFilter(pageSize, len);
        validFilter.SetCurrentPage(pageNumber);
        var response = await responseQuery
             .Skip((validFilter.CurrentPage - 1) * validFilter.PageSize)
             .Take(validFilter.PageSize)
             .ToListAsync();                   
        
        return Ok(new
        {
            currentPage = validFilter.CurrentPage,
            lastPage = validFilter.LastPage,
            posts = response
        });
    }
}