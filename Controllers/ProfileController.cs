using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearnApi.Models;
using Microsoft.AspNetCore.Authorization;
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


        private bool ProfileExists(long id)
        {
            return (_context.Profiles?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
