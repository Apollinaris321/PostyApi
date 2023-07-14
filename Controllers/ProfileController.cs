using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearnApi.Models;
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
        [Route("home")]
        public async Task<IActionResult> HomeFeed(string sort = "new")
        {
            if (sort != "new" && sort != "popular")
            {
                return BadRequest("Sort parameter not allowed!");
            }
            var username = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
            if (sort == "new")
            {
                var posts = _context.Posts
                    .Where(p => p.Profile.UserName == username)
                    .OrderBy(p => p.CreatedAt)
                    .Select(p => new PostDto(p)).ToListAsync();
                return Ok(posts);
            }
            else
            {
                 var posts = _context.Posts
                     .Where(p => p.Profile.UserName == username)
                     .OrderBy(p => p.Likes)
                     .Select(p => new PostDto(p)).ToListAsync();
                 return Ok(posts);               
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllProfiles()
        {
            var profileList = await _context.Profiles.Select(p => new ProfileDto(p)).ToListAsync();
            return Ok(profileList);
        }

        [HttpGet]
        [Route("{username}/comments")]
        public async Task<IActionResult> GetProfileComments(string username)
        {
            var comments = await _context.Comments
                .Where(c => c.Profile.UserName == username)
                .Select(c => new CommentDto(c))
                .ToListAsync();
            return Ok(comments);
        }
        
        [HttpGet]
        [Route("{username}/posts")]
        public async Task<IActionResult> GetPostsByProfileId(string username)
        {
            var posts = await _context.Posts
                .Include(p => p.Profile)
                .Where(post => post.Profile.UserName == username)
                .Select(post => new PostDto(post))
                .ToListAsync();
            return Ok(posts);
        }
        
        [Authorize]
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
        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteProfile(string username)
        {
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