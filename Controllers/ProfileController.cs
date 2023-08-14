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
        private readonly IProfileService _profileService;
        private readonly ICommentService _commentService;
        private readonly IPostService _postService;

        public ProfileController(
            IProfileService profileService,
            ICommentService commentService,
            IPostService postService
            )
        {
            _profileService = profileService;
            _commentService = commentService;
            _postService = postService;
        }
       
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllProfiles()
        {
            var response = await _profileService.GetAll();
            return Ok(response.Data);
        }

        [HttpGet]
        [Route("{username}/comments")]
        public async Task<IActionResult> GetProfileComments(string username, int pageNumber = 1, int pageSize = 10)
        {
            var response = await _commentService.GetByUsername(username, pageNumber, pageSize);
            return Ok(new
            {
                currentPage = response.CurrentPage,
                lastPage = response.LastPage,
                comments = response.Data
            });
        }
        
        [HttpGet]
        [Route("{username}/posts")]
        public async Task<IActionResult> GetProfilePosts(string username, int pageSize = 10, int pageNumber = 1)
        {
            var response = await _postService.GetByUsername(username, pageNumber, pageSize);
            
            return Ok(new
            {
                currentPage = response.CurrentPage,
                lastPage = response.LastPage,
                posts = response.Data
            });
        }
        
        [HttpGet("{username}")]
        public async Task<ActionResult<Profile>> GetProfile(string username)
        {
            var sessionId = HttpContext.Request.Cookies["Auth"];
            var profile = await _profileService.GetByUsername(username);
            return Ok(profile);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfile(long id)
        {
            var sessionId = HttpContext.Request.Cookies["Auth"];
            var response = await _profileService.Delete(id, sessionId);
            if (response.Success)
            {
                return Ok();
            }
            else
            {
                return NotFound(response.Error);
            }
        }       
    }
}