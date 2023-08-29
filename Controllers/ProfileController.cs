using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearnApi.Models;
using LearnApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace LearnApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly PostyContext _context;
        private readonly UserManager<Profile> _userManager;
        private readonly SignInManager<Profile> _signInManager;
        private readonly IProfileService _profileService;

        public ProfileController(
            PostyContext context,
            UserManager<Profile> userManager,
            SignInManager<Profile> signInManager,
            IProfileService profileService
            )
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _profileService = profileService;
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
                return Ok(newProfile);
            }
            return BadRequest(registerResult.Errors);
        }
        
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var signInResult = await _signInManager.PasswordSignInAsync(loginDto.Username, loginDto.Password, false ,false);
            if (signInResult.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(loginDto.Username);
                return Ok(new ProfileDto(user));
            }
            return BadRequest(new {errors = new {Login = "Wrong password or Username!"}});
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            var result = _signInManager.IsSignedIn(HttpContext.User);
            if (result)
            {
                await _signInManager.SignOutAsync();
                return Ok();
            }
            return BadRequest("Can only logout if logged in!");
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllProfiles(int pageNumber = 1, int pageSize = 10)
        {
            var profiles = await _profileService.GetAllProfiles(pageNumber, pageSize);
            return Ok(profiles.Data);
        }

        [HttpGet]
        [Route("{username}/comments")]
        public async Task<IActionResult> GetProfileComments(string username, int take = 1, int offset = 10)
        {
            var results = await _profileService.GetProfileComments(username, take, offset);
            if (results.Success)
            {
                return Ok(results.Data);
            }
            return BadRequest(results.Errors);
        }
        
        [HttpGet]
        [Route("{username}/posts")]
        public async Task<IActionResult> GetProfilePosts(string username, int take = 10, int offset = 1)
        {
             var results = await _profileService.GetProfilePosts(username, take, offset);
             if (results.Success)
             {
                 return Ok(results.Data);
             }
             return BadRequest(results.Errors);
        }
        
        [HttpGet("{username}")]
        public async Task<ActionResult<Profile>> GetProfile(string username)
        {
            var results = await _profileService.GetProfile(username);
            if (results.Success)
            {
                return Ok(results.Data);
            }
            return BadRequest(results.Errors);
        }
        
        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteProfile(string username)
        {
            var results = await _profileService.DeleteProfile(username);
            if (results.Success)
            {
                return Ok(results.Data);
            }
            return BadRequest(results.Errors);           
        }       
    }
}