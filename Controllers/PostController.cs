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
    private readonly PostyContext _context;
    private readonly IConfiguration _configuration;
    private readonly UserManager<Profile> _userManager;

    public PostController(PostyContext context, IConfiguration configuration, UserManager<Profile> userManager)
    {
        _context = context;
        _configuration = configuration;
        _userManager = userManager;
    }

    [HttpGet]
    [Route("feed")]
    public async Task<IActionResult> Feed(int pageSize = 10, int pageNumber = 1, string order = "date")
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var len = _context.Posts.Count();
        var validFilter = new PaginationFilter(pageSize, len);       
        validFilter.SetCurrentPage(pageNumber);

        if (order == "date")
        {
            var response = await _context.Posts
                .Include(post => post.Profile)
                .Include(post => post.ProfileLikes)
                .OrderByDescending(p => p.CreatedAt)
                .Select(post => new PostDto(post, long.Parse(userId)))
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
        else if (order == "likes")
        {
             var response = await _context.Posts
                 .Include(post => post.Profile)
                 .Include(post => post.ProfileLikes)
                 .OrderByDescending(p => p.CreatedAt)
                 .Select(post => new PostDto(post, long.Parse(userId)))
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

        return BadRequest($"Order can be either likes or date not {order}");
    }
    
    [HttpGet]
    [Route("{postId}/comments")]
    public async Task<IActionResult> GetComments(long postId, int pageSize = 10, int pageNumber = 1, string order = "likes")
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var len = _context.Comments
            .Count();
        var validFilter = new PaginationFilter(pageSize, len);
        validFilter.SetCurrentPage(pageNumber);

        if (order == "likes")
        {
            var comments = await _context.Comments
                .Include(comment => comment.Profile)
                .Include(comment => comment.LikedBy)
                .Where(comment => comment.PostId == postId)
                .OrderByDescending(comment => comment.Likes)
                .Select(c =>  new CommentDto(c, userId))
                .Skip((validFilter.CurrentPage - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToListAsync();
 
            return Ok(new { currentPage = validFilter.CurrentPage, lastPage = validFilter.LastPage, comments = comments });           
        }
        else if(order == "date")
        {
             var comments = await _context.Comments
                 .Include(comment => comment.Profile)
                 .Include(comment => comment.LikedBy)
                 .Where(comment => comment.PostId == postId)
                 .OrderByDescending(comment => comment.CreatedAt)
                 .Select(c =>  new CommentDto(c, userId))
                 .Skip((validFilter.CurrentPage - 1) * validFilter.PageSize)
                 .Take(validFilter.PageSize)
                 .ToListAsync();
  
             return Ok(new { currentPage = validFilter.CurrentPage, lastPage = validFilter.LastPage, comments = comments });                      
        }
        return BadRequest("Order can be either date or likes. Yours was: " + order);
    }

    // [HttpGet]
    // [Route("{postId}/likes")]
    // public async Task<IActionResult> GetLikes(long postId)
    // {
    //     var likes = await _context.PostLikes
    //         .Include(like => like.Profile)
    //         .Where(like => like.PostId == postId)
    //         .Select(l => new { Username = l.Profile.Username})
    //         .ToListAsync();
    //     return Ok(likes);
    // }
    
    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> getById(long id)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var post = await _context.Posts
            .Include(post => post.Profile)
            .Include(post => post.ProfileLikes)
            .SingleOrDefaultAsync(p => p.Id == id);
        if (post == null)
        {
            return BadRequest($"Post with id: {id} doesn't exist!");
        }
 
        return Ok(new PostDto(post, long.Parse(userId)));
    }
 
    [HttpPost]
    public async Task<IActionResult> create(CreatePostDto postDto)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return BadRequest("Register or Login to create posts!");
        }
         
        var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.Id.ToString() == userId);
        if (profile == null)
        {
            return BadRequest("Couldnt find profile");
        }
        
        var newPost = new Post
        {
            Profile = profile,
            Text = postDto.Text
        };
 
        await _context.Posts.AddAsync(newPost);
        await _context.SaveChangesAsync();
        return Ok(new PostDto(newPost));
    }   
     
    [HttpPost]
    [Route("{postId}/comments")]
    public async Task<IActionResult> AddComment(long postId,CreateCommentDto commentPayload)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return BadRequest("Couldnt find your id");
        }       
        
        var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.Id.ToString() == userId);
        var post = await _context.Posts.SingleOrDefaultAsync(p => p.Id == postId);
        if (profile == null || post == null)
        {
            return BadRequest("Couldnt find profile or post");
        }       
        
        var newComment = new Comment
        {
            Profile = profile,
            Post = post,
            Text = commentPayload.Text
        };

        await _context.Comments.AddAsync(newComment);
        await _context.SaveChangesAsync();
        return Ok(new CommentDto(newComment));
    }

    [HttpPost]
    [Route("{postId}/likes")]
    public async Task<IActionResult> Like(long postId)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return BadRequest("Couldnt find your id");
        }       
         
        var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.Id.ToString() == userId);
        var post = await _context.Posts.SingleOrDefaultAsync(p => p.Id == postId);
        if (profile == null || post == null)
        {
            return BadRequest("Couldnt find profile or post");
        }
        
        var newLike = new PostLike
        {
            ProfileId = profile.Id,
            PostId = post.Id
        };

        try
        {
            await _context.PostLikes.AddAsync(newLike);
            await _context.SaveChangesAsync();
            return Ok();
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
            .SingleOrDefaultAsync(p => p.Id == id && p.Profile.Username == username );
        if (post == null)
        {
            return BadRequest($"Post with id: {id} doesn't exist or it's not your post!");
        }

        try
        {
            post.Text = postDto.Text;
            await _context.SaveChangesAsync();
            return Ok(new PostDto(post));
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
            .SingleOrDefaultAsync(p => p.Id == id && p.Profile.Username == username);
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

    [HttpDelete]
    [Route("{postId}/likes")]
    public async Task<IActionResult> Dislike(long postId)
    {
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var like = await _context.PostLikes
            .SingleOrDefaultAsync(like => like.ProfileId == long.Parse(userId) && like.PostId == postId);
        if (like == null)
        {
            return BadRequest($"You can only dislike things you liked before!");
        }

        _context.PostLikes.Remove(like);
        await _context.SaveChangesAsync();
        return Ok();
    }
}