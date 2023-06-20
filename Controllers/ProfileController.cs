using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearnApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LearnApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProfileController(TodoContext context,IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        
        private string CreateToken(Profile profile)
        {
            List<Claim> claims = new List<Claim> {
                new Claim(ClaimTypes.Name, profile.Username),
                new Claim(ClaimTypes.Role, "User"),
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
        public async Task<IActionResult> Register(ProfileRegisterDto profileR)
        {
            var result =
                await _context.Profiles.FirstOrDefaultAsync(p =>
                    p.Username == profileR.Username || p.Email == profileR.Email);
            
            if (result != null)
            {
                return Conflict("User already exists!");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(profileR.Password);

            var newProfile = new Profile(profileR);
            newProfile.Password = passwordHash;

            _context.Profiles.Add(newProfile);
            await _context.SaveChangesAsync();

            return Ok(newProfile);
        }
        
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] ProfileLoginDto profileL)
        {
            var profileFound = await _context.Profiles.Include(p => p.Worksheets).FirstOrDefaultAsync(p => p.Username == profileL.Username);

            if (profileFound == null)
            {
                return NotFound();
            }

            if (BCrypt.Net.BCrypt.Verify(profileL.Password, profileFound.Password))
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

            return BadRequest(new {message = "wrong password!"});
        }

        [HttpPost]
        [Route("{id}/worksheet")]
        public async Task<ActionResult> SaveWorksheet([FromBody]WorksheetDto worksheetDto,int id)
        {
            Console.WriteLine("received worksheet");
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.Id == id);
            if(profile is null)
            {
                return BadRequest(new {message = "failed to find profile!"});
            }

            Worksheet worksheet = new Worksheet(worksheetDto);
            EntityEntry<Worksheet> savedWorksheet = await _context.Worksheets.AddAsync(worksheet);
            profile.Worksheets.Add(savedWorksheet.Entity);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return Ok(savedWorksheet.Entity);
            }
            return BadRequest(new {message = "failed to save worksheet!"});
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Profile>>> GetProfiles()
        {
            return await _context.Profiles.Include(p => p.Worksheets).ToListAsync();
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

        // POST: api/Profile
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Profile>> PostProfile(Profile profile)
        {
            //_context.Profiles.Add(profile);
            // var newProfile = await _userManager.CreateAsync(
            //     new IdentityUser() { UserName = profile.Username, Email = profile.Email }, profile.Password
            // );
            
            //return CreatedAtAction("GetProfile", newProfile);
            return Ok();
        }

        // DELETE: api/Profile/5
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
