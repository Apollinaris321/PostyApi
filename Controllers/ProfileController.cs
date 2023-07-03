using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearnApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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

        public ProfileController(TodoContext context,IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        
        private string CreateToken(Profile profile)
        {
            List<Claim> claims = new List<Claim> {
                new Claim(ClaimTypes.Name, profile.Username),
                new Claim("ProfileId", profile.Id.ToString()),
                new Claim(ClaimTypes.Email, profile.Email),
                new Claim(ClaimTypes.Role, "User")
            };
 
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));
 
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
 
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );
 
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
 
            return jwt;
        }

        [HttpGet]
        [Route("/commentLikes")]
        public async Task<IActionResult> getCommentLikes()
        {
            return Ok(await _context.CommentLikes.ToListAsync());
        }

        [HttpPost]
        // das ist doch bescheuert. eigentlich müsste es pId/post/cId/comment sein oder so
        [Route("{profileId}/profile/{commentId}/comment")]
        public async Task<IActionResult> likeComment(long profileId, long commentId)
        {
            var comment = await _context.Comments.SingleOrDefaultAsync(c => c.Id == commentId);
            var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.Id == profileId);

            if (comment == null || profile == null)
            {
                return BadRequest("Comment or profile doesn't exist!");
            }

            var newLike = new CommentLike
            {
                Comment = comment,
                Profile = profile
            };

            _context.CommentLikes.Add(newLike);
            await _context.SaveChangesAsync();
            return Ok(newLike);
        }

        [HttpDelete]
        [Route("{profileId}/profile/{commentId}/commentlike")]
        public async Task<IActionResult> deleteCommentLike(long profileId, long commentId)
        {
            var like = await _context.CommentLikes.SingleOrDefaultAsync(c =>
                c.ProfileId == profileId && c.CommentId == commentId);
            if (like == null)
            {
                return BadRequest("You didn't like this comment!");
            }

            var comment = await _context.Comments.SingleOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
            {
                return BadRequest("Comment doesn't exist");
            }
            comment.Likes--;
            _context.CommentLikes.Remove(like);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Route("{profileId}/profile/{postId}/post/like")]
        public async Task<IActionResult> postLike(long profileId, long postId)
        {
            var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.Id == profileId);
            var post = await _context.Posts.SingleOrDefaultAsync(p => p.Id == postId);
            if (profile == null || post == null)
            {
                return BadRequest("User or post doesn't exist!");
            }
            
            var newLike = new PostLike
            {
                Profile = profile,
                Post = post,
            };
            await _context.Likes.AddAsync(newLike);
            profile.LikedPosts.Add(newLike);
            post.ProfileLikes.Add(newLike);
            await _context.SaveChangesAsync();
            
            return Ok(newLike);
        }

        [HttpDelete]
        [Route("{profileId}/profile/{postId}/post/like")]
        public async Task<IActionResult> deleteLike(long postId,long profileId)
        {
            var result = await _context.Likes.SingleOrDefaultAsync(l => l.ProfileId == profileId && l.PostId == postId);
            if (result == null)
            {
                return BadRequest("You didnt like this post!");
            }

            _context.Likes.Remove(result);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        [Route("/post/like")]
        public async Task<IActionResult> getLikes()
        {
            return Ok(await _context.Likes.Include(l => l.Profile).Include(l => l.Post).ToListAsync());
        }

        [HttpDelete]
        [Route("{postId}/post/{commentId}/comment")]
        public async Task<IActionResult> DeleteComment(long postId, long commentId)
        {
            var comment = await _context.Comments.SingleOrDefaultAsync(c => c.PostId == postId && c.Id == commentId);
            if (comment == null)
            {
                return BadRequest("Comment doesnt exist");
            }
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        [Route("{id}/profile/comments")]
        public async Task<IActionResult> GetProfileComments(long id)
        {
            var comments = await _context.Comments.Include(c => c.Profile).Include(c => c.Post).Where(c => c.ProfileId == id).ToListAsync();
            return Ok(comments);
        }
        
        [HttpGet]
        [Route("{id}/post/comments")]
        public async Task<IActionResult> GetPostComments(long id)
        {
            var comments = await _context.Comments.Where(c => c.PostId == id).ToListAsync();
            return Ok(comments);
        }
        
        [HttpGet]
        [Route("/comments")]
        public async Task<IActionResult> GetComments()
        {
            var comments = await _context.Comments.Include(c => c.Profile).Include(c => c.Post).ToListAsync();
            return Ok(comments);
        }
        
        // I need a post controller for this : {id}/post/comments
        [HttpPost]
        [Route("{postId}/post/comments")]
        public async Task<IActionResult> AddComment(long? postId, long profileId, string text)
        {
            if (postId == null)
            {
                return BadRequest("empty postId");
            }

            var post = await _context.Posts.SingleOrDefaultAsync(p => p.Id == postId);
            var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.Id == profileId);

            if (post == null || profile == null)
            {
                return BadRequest("Post or Profile doesn't exist!");
            }
            
            var comment = new Comment
            {
                Post = post,
                Text = text,
                Profile = profile 
            };
            
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return Ok(comment);
        }

        [HttpDelete]
        [Route("{profileId}/post/{postId}")]
        public async Task<IActionResult> DeletePost(long? profileId, long? postId)
        {
            if (profileId == null || postId == null)
            {
                return BadRequest("No id's provided!");
            }
            var post = await _context.Posts.SingleOrDefaultAsync(p => p.Id == postId);
            if (post == null)
            {
                return BadRequest("This post doesn't exist!");
            }

            if (post.ProfileId != profileId)
            {
                return Unauthorized("Cannot delete other peoples posts!");
            }
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return Ok("success");
        }

        [HttpPost]
        [Route("{id}/post")]
        public async Task<IActionResult> AddPost(long? id, PostDto post)
        {
            if (id == null || id != post.ProfileId)
            {
                return BadRequest();
            }

            try
            {
                var ownerProfile = await _context.Profiles.FirstOrDefaultAsync(p => p.Id == post.ProfileId);
                if (ownerProfile == null)
                {
                    return BadRequest($"Owner doesn't exist! Owner profileId: {post.ProfileId}");
                }

                //var newPost = new Post(ownerProfile.Id, post.Text);
                var newPost = new Post(post.Text, ownerProfile);
                var postResult = await _context.Posts.AddAsync(newPost);
                //ownerProfile.Posts.Add(postResult.Entity);
                await _context.SaveChangesAsync();
                return Ok(postResult.Entity);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("allPosts")]
        public async Task<IActionResult> GetAllPosts()
        {
            var postList = await _context.Posts.Include(p => p.Profile).ToListAsync();
            return Ok(postList);
        }
        
        [HttpGet]
        [Route("allProfiles")]
        public async Task<IActionResult> GetAllProfiles()
        {
            var profileList = await _context.Profiles.ToListAsync();
            return Ok(profileList);
        }

        [HttpGet]
        [Route("{id}/posts")]
        public async Task<IActionResult> GetPostsByProfileId(long? id)
        {
            if (id == null)
            {
                return BadRequest($"Id cannot be empty!");
            }           
            var profile = await _context.Posts.Include(p => p.Profile).Where(post => post.ProfileId == id).ToListAsync();
            return Ok(profile);
        }

        [HttpPost]
        [Route("addProfile")]
        public async Task<IActionResult> AddProfile(ProfileDto profileDto)
        {
            try
            {
                var profile = new Profile(profileDto.Username, profileDto.Email, profileDto.Password );
                var newProfile = await _context.Profiles.AddAsync(profile);
                await _context.SaveChangesAsync();
                return Ok(newProfile.Entity);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<ProfileDto>> Register(RegisterDto registerDto)
        {
            var result =
                await _context.Profiles.FirstOrDefaultAsync(p =>
                    p.Username == registerDto.Username || p.Email == registerDto.Email);
            
            if (result != null)
            {
                return Conflict("User already exists!");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var newProfile = new Profile(registerDto.Username, registerDto.Email, registerDto.Password);
            newProfile.Password = passwordHash;

            _context.Profiles.Add(newProfile);
            await _context.SaveChangesAsync();
            
            string token = CreateToken(newProfile);
            Response.Cookies.Append("jwt", token,  new CookieOptions()
            {
                HttpOnly = true,
                Expires = DateTimeOffset.Now.AddDays(10),
                IsEssential = true,
                SameSite = SameSiteMode.None ,
                Secure = true
            });
            return Ok(newProfile);
        }
        
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var profileFound = await _context.Profiles.Include(p => p.Worksheets).FirstOrDefaultAsync(p => p.Username == loginDto.Username);

            if (profileFound == null)
            {
                return NotFound("User with this name doesn't exist!");
            }

            if (BCrypt.Net.BCrypt.Verify(loginDto.Password, profileFound.Password))
            {
                string token = CreateToken(profileFound);
                Response.Cookies.Append("jwt", token,  new CookieOptions()
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddDays(10),
                    IsEssential = true,
                    SameSite = SameSiteMode.None ,
                    Secure = true
                });
                return Ok(profileFound);
            }

            return BadRequest("wrong password!");
        }

        [Authorize]
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt", new CookieOptions 
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true
            });
            return Ok(new { message = "log out successful!" });
        }
         
        [HttpGet]
        [Authorize]
        [Route("{id}/worksheet")]
        public async Task<ActionResult> GetAllWorksheet(long? id)
        {
            var profileId = int.Parse(HttpContext.User.FindFirstValue("ProfileId") ?? string.Empty);
            if(id != profileId)
            {
                return Unauthorized("Cannot access other users data!");
            }
            List<Worksheet> worksheets = await _context.Worksheets.Where(w => w.ProfileId == profileId).ToListAsync();
            return Ok(worksheets);
        }        
          
        [HttpGet]
        [Authorize]
        [Route("{id}/worksheet/{worksheetId}")]
        public async Task<ActionResult> GetWorksheet(long? id,long?  worksheetId)
        {
            if (worksheetId == null)
            {
                return BadRequest("Worksheet id is null!");
            }
            var profileId = int.Parse(HttpContext.User.FindFirstValue("ProfileId") ?? string.Empty);
            if(id != profileId)
            {
                return Unauthorized("Cannot access other users data!");
            }
            Worksheet? worksheet = await _context.Worksheets.FirstOrDefaultAsync(w => w.ProfileId == profileId && w.Id == worksheetId);
            if (worksheet != null)
            {
                return Ok(worksheet);
            }

            return NotFound($"No worksheet with this id {worksheetId}");
        }               
        
        [HttpPut]
        [Authorize]
        [Route("{id}/worksheet/{worksheetId}")]
        public async Task<ActionResult> PostWorksheet(long id,long worksheetId, Worksheet worksheet)
        {
            var profileId = int.Parse(HttpContext.User.FindFirstValue("ProfileId") ?? string.Empty);
            if(id != profileId)
            {
                return Unauthorized("Cannot access other users data!");
            }

            if (worksheetId != worksheet.Id)
            {
                return BadRequest($"worksheet id's not matching! param: {worksheetId} body: {worksheet.Id} ");
            }

            var oldWorksheet = await _context.Worksheets.FirstOrDefaultAsync(w => w.Id == worksheet.Id);
            _context.Entry(oldWorksheet).CurrentValues.SetValues(worksheet);
            var result = await _context.SaveChangesAsync();
            return Ok(worksheet);
        }       
        
        [HttpPost]
        [Authorize]
        [Route("{id}/worksheet")]
        public async Task<ActionResult> PostWorksheet(long id, WorksheetDto worksheetDto)
        {
            var profileId = int.Parse(HttpContext.User.FindFirstValue("ProfileId") ?? string.Empty);
            if(id != profileId)
            {
                return Unauthorized("Cannot access other users data!");
            }

            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.Id == profileId);

            if (profile != null)
            {
                var newWorksheet = new Worksheet(worksheetDto.Title, worksheetDto.Exercises, worksheetDto.ProfileId);
                await _context.Worksheets.AddAsync(newWorksheet);
                profile.Worksheets.Add(newWorksheet);
                await _context.SaveChangesAsync();
                return Ok(newWorksheet);
            }
            else
            {
                return BadRequest("Could not find your user profile! ");
            }
        }

        
        [HttpDelete]
        [Authorize]
        [Route("{id}/worksheet/{worksheetId}")]
        public async Task<ActionResult> DeleteWorksheet(long id, long worksheetId)
        {
            var profileId = int.Parse(HttpContext.User.FindFirstValue("ProfileId") ?? string.Empty);
            if(id != profileId)
            {
                return Unauthorized("Cannot access other users data!");
            }

            var tempWorksheet = new Worksheet() { Id = worksheetId };
            _context.Worksheets.Remove(tempWorksheet);
            await _context.SaveChangesAsync();
            return Ok(tempWorksheet.Id);
        }
        
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Profile>> GetProfile(long id)
        {
            if (_context.Profiles == null)
            {
                return NotFound();
            }
            var profile = await _context.Profiles.Include(p => p.Worksheets).FirstOrDefaultAsync(p => p.Id == id);
            if (profile == null)
            {
                return NotFound();
            }
            return profile;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfile(long id)
        {
            //var profileId = int.Parse(HttpContext.User.FindFirstValue("ProfileId") ?? string.Empty);
            // if (profileId != id)
            // {
            //     return Unauthorized($"You can only delete your own profile! userid: {profileId}, paramId: {id}");
            // }
            
            if (_context.Profiles == null)
            {
                return NotFound();
            }
            var profile = await _context.Profiles.Include(p => p.Worksheets).FirstOrDefaultAsync(p => p.Id == id);
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