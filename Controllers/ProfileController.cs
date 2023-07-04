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
        public async Task<IActionResult> GetAllProfiles()
        {
            var profileList = await _context.Profiles.ToListAsync();
            var username = HttpContext.Session.GetString("username");
            var id = HttpContext.Session.GetInt32("id");
            Console.WriteLine($"username: {username}, id: {id}");
            return Ok(new{list = profileList, username = username, id = id});
        }

        [HttpGet]
        [Route("{id}/comments")]
        public async Task<IActionResult> GetProfileComments(long id)
        {
            var comments = await _context.Comments.Include(c => c.Profile).Include(c => c.Post).Where(c => c.ProfileId == id).ToListAsync();
            return Ok(comments);
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
                HttpContext.Session.SetInt32("id", (int)profileFound.Id);
                HttpContext.Session.SetString("username", profileFound.Username);
                return Ok(profileFound);
            }

            return BadRequest("wrong password!");
        }

        [Authorize]
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            // Response.Cookies.Delete("jwt", new CookieOptions 
            // {
            //     HttpOnly = true,
            //     SameSite = SameSiteMode.None,
            //     Secure = true
            // });
            HttpContext.Session.Clear();
            HttpContext.Response.Cookies.Delete("TestApi");
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