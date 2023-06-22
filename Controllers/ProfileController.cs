using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearnApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
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
        [Authorize(Roles="User")]
        [Route("getmyname")]
        public IActionResult GetMyProfile()
        {
            var name = HttpContext.User.FindFirstValue(ClaimTypes.Name);
            var profile = _context.Profiles.Include(p => p.Worksheets).FirstOrDefaultAsync(p => p.Username == name);

            if (profile.Result == null)
            {
                return NotFound("No Profile with this username found!");
            }
            return Ok(profile.Result);
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


        [HttpPost]
        [Route("register")]
        public async Task<ActionResult<ProfileDto>> Register(ProfileDto pDto)//string username,string email = "e",string password = "e")
        {
            var result =
                await _context.Profiles.FirstOrDefaultAsync(p =>
                    p.Username == pDto.Username || p.Email == pDto.Email);
            
            if (result != null)
            {
                return Conflict("User already exists!");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(pDto.Password);

            var newProfile = new Profile(pDto.Username, pDto.Email, pDto.Password);
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
                return NotFound();
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

        [HttpPut]
        [Authorize]
        [Route("{id}/worksheet/{worksheetId}")]
        public async Task<ActionResult> PutWorksheet(long id,long worksheetId, Worksheet worksheet)
        {
            var profileId = int.Parse(HttpContext.User.FindFirstValue("ProfileId") ?? string.Empty);
            if(id != profileId)
            {
                return Unauthorized("Cannot access other users data!");
            }
            Worksheet? oldWorksheet = await _context.Worksheets.FirstOrDefaultAsync(w => w.ProfileId == profileId && w.Id == worksheetId);
            EntityEntry<Worksheet> newWorksheet = null;
            if (oldWorksheet is null)
            {
                newWorksheet = await _context.Worksheets.AddAsync(worksheet);
                await _context.SaveChangesAsync();
                return Ok(newWorksheet.Entity);
            }
            _context.Worksheets.Remove(oldWorksheet);
            newWorksheet = await _context.Worksheets.AddAsync(worksheet);
            await _context.SaveChangesAsync();
            return Ok(newWorksheet.Entity);
        }
 
        [HttpDelete]
        [Authorize]
        [Route("{id}/worksheet/{worksheetId}")]
        public async Task<ActionResult> DeleteWorksheet(long id,long worksheetId)
        {
            var profileId = int.Parse(HttpContext.User.FindFirstValue("ProfileId") ?? string.Empty);
            if(id != profileId)
            {
                return Unauthorized("Cannot access other users data!");
            }
            
            Worksheet? worksheet = await _context.Worksheets.FirstOrDefaultAsync(w => w.ProfileId == profileId && w.Id == worksheetId);
            EntityEntry<Worksheet> newWorksheet = null;
            if (worksheet is null)
            {
                return BadRequest("Worksheet with this id doesn't exist!");
            }
            _context.Worksheets.Remove(worksheet);
            await _context.SaveChangesAsync();
            return Ok("Successfully removed Worksheet");
        }       
        
        [HttpGet]
        [Authorize]
        [Route("{id}/worksheet")]
        public async Task<ActionResult> GetWorksheet(long? id)
        {
            var profileId = int.Parse(HttpContext.User.FindFirstValue("ProfileId") ?? string.Empty);
            if(id != profileId)
            {
                return Unauthorized("Cannot access other users data!");
            }
            List<Worksheet> worksheets = await _context.Worksheets.Where(w => w.ProfileId == profileId).ToListAsync();
            return Ok(worksheets);
        }
        
        [HttpPut]
        [Route("{id}/worksheet")]
        public async Task<ActionResult> SaveWorksheet(long? id,Worksheet worksheetDto)
        {
            var username = HttpContext.User.FindFirstValue(ClaimTypes.Name); 
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.Username == username);
            if(profile is null)
            {
                return BadRequest(new {message = "failed to find profile!"});
            }

            Worksheet worksheet = new Worksheet(worksheetDto.Title, worksheetDto.Exercises, profile.Id);
            EntityEntry<Worksheet> savedWorksheet = await _context.Worksheets.AddAsync(worksheetDto);
            profile.Worksheets.Add(savedWorksheet.Entity);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok(savedWorksheet.Entity);
            }
            return BadRequest("failed to save worksheet!");
        }

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

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProfile(long id, Profile profile)
        {
            if (id != profile.Id)
            {
                return BadRequest();
            }

            _context.Entry(profile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfileExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        
        
        //////////////////////

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Profile>>> GetProfiles()
        {
            return await _context.Profiles.Include(p => p.Worksheets).ToListAsync();
        }
        
        [HttpPost]
        public async Task<ActionResult<Profile>> PostProfile(Profile profile)
        {
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfile(long id)
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

            _context.Profiles.Remove(profile);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProfileExists(long id)
        {
            return (_context.Profiles?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
