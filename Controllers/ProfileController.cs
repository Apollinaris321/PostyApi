using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearnApi.Models;
using LearnApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.IdentityModel.Tokens;

namespace LearnApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<Profile> _userManager;
        private readonly SignInManager<Profile> _signInManager;

        public ProfileController(
            TodoContext context,
            IConfiguration configuration,
            UserManager<Profile> userManager,
            SignInManager<Profile> signInManager)
        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<ProfileDto>> Register(RegisterDto registerDto)
        {
            var newProfile = new Profile{
                UserName = registerDto.Username, 
                Email = registerDto.Email, 
                
            };
            var registerResult = await _userManager.CreateAsync(newProfile, registerDto.Password);
            if (registerResult.Succeeded)
            {
                await _signInManager.SignInAsync(newProfile, isPersistent: false);
                return Ok(new ProfileDto(newProfile));
            }

            var errorString = "";
            foreach (var error in registerResult.Errors)
            {
                errorString += error.Description + " ,";
            }
            return BadRequest(new {errors = new {Register = $"Failed to register: {errorString}"}});
        }
        
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user != null)
            {
                var signInResult = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false ,false);
                if (signInResult.Succeeded)
                {
                    return Ok(new ProfileDto(user));
                }
            }
            return BadRequest(new {errors = new {Login = "Wrong password or Username!"}});
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllProfiles()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profile = await _context.Profiles
                .SingleOrDefaultAsync(p => p.Id == userId);
            return Ok(new ProfileDto(profile));
        }

        [HttpGet]
        [Route("{username}/comments")]
        public async Task<IActionResult> GetProfileComments(string username, int pageNumber = 1, int pageSize = 10)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var len = _context.Comments
                .Count(comment => comment.Profile.UserName == username);
            var validFilter = new PaginationFilter(pageSize, len);
            validFilter.SetCurrentPage(pageNumber);
                         
            var comments = await _context.Comments
                .Include(c => c.Profile)
                .Include(c => c.LikedBy)
                .Where(c => c.Profile.UserName == username)
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
                .Count(post => post.Profile.UserName == username);
            var validFilter = new PaginationFilter(pageSize, len);
            validFilter.SetCurrentPage(pageNumber);
             
            var posts = await _context.Posts
                .Include(p => p.Profile)
                .Include(p => p.ProfileLikes)
                .Where(post => post.Profile.UserName == username)
                .OrderByDescending(post => post.CreatedAt)
                .Select(post => new PostDto(post, userId))
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
            var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.UserName == username);
            if (profile == null)
            {
                return NotFound("error could not find profile!");
            }
            return Ok(new ProfileDto(profile));
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
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.UserName == username);
            if (profile == null)
            {
                return NotFound();
            }

            _context.Profiles.Remove(profile);
            await _context.SaveChangesAsync();

            return NoContent();
        }       
    }
}