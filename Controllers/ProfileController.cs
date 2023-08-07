using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearnApi.Models;
using LearnApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace LearnApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly PostyContext _context;

        public ProfileController(
            PostyContext context
            )
        {
            _context = context;
        }
       
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllProfiles()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profile = await _context.Profiles
                .SingleOrDefaultAsync(p => p.Id.ToString() == userId);
            return Ok(new ProfileDto(profile));
        }

        [HttpGet]
        [Route("{username}/comments")]
        public async Task<IActionResult> GetProfileComments(string username, int pageNumber = 1, int pageSize = 10)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var len = _context.Comments
                .Count(comment => comment.Profile.Username == username);
            var validFilter = new PaginationFilter(pageSize, len);
            validFilter.SetCurrentPage(pageNumber);
                         
            var comments = await _context.Comments
                .Include(c => c.Profile)
                .Include(c => c.LikedBy)
                .Where(c => c.Profile.Username == username)
                .Select(c => new CommentDto(c, userId))
                .Skip((validFilter.CurrentPage - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToListAsync();
            return Ok(new
            {
                currentPage = validFilter.CurrentPage,
                lastPage = validFilter.LastPage,
                comments = comments
            });
        }
        
        [HttpGet]
        [Route("{username}/posts")]
        public async Task<IActionResult> GetProfilePosts(string username, int pageSize = 10, int pageNumber = 1)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var len = _context.Posts
                .Count(post => post.Profile.Username == username);
            var validFilter = new PaginationFilter(pageSize, len);
            validFilter.SetCurrentPage(pageNumber);
             
            var posts = await _context.Posts
                .Include(p => p.Profile)
                .Include(p => p.ProfileLikes)
                .Where(post => post.Profile.Username == username)
                .OrderByDescending(post => post.CreatedAt)
                .Select(post => new PostDto(post, long.Parse(userId)))
                .Skip((validFilter.CurrentPage - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .ToListAsync();
            
            return Ok(new
            {
                currentPage = validFilter.CurrentPage,
                lastPage = validFilter.LastPage,
                posts = posts
            });
        }
        
        [HttpGet("{username}")]
        public async Task<ActionResult<Profile>> GetProfile(string username)
        {
            var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.Username == username);
            if (profile == null)
            {
                return NotFound("error could not find profile!");
            }
            return Ok(profile);
        }
        
        // TODO can only delete your own profile
        // i keep it here for testing...
        //[Authorize]
        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteProfile(string username)
        {
            var contextUsername = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if (contextUsername != username)
            {
                //return BadRequest("Can't delete other peoples profiles!");
            }
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.Username == username);
            if (profile == null)
            {
                return NotFound();
            }

            _context.Profiles.Remove(profile);

            return NoContent();
        }       
    }
}