using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearnApi.Models;
using LearnApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace LearnApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtService _jwtService;


        public ProfileController(TodoContext context, UserManager<IdentityUser> userManager, JwtService jwtService)
        {
            _context = context;
            _userManager = userManager;
            _jwtService = jwtService;
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
                if (result.Username == profileR.Username)
                {
                    return Conflict("Username already in use!");
                }
                if (result.Email == profileR.Email)
                {
                    return Conflict("Email already in use!");
                }
            }

            var newP = await _userManager.CreateAsync(
                new IdentityUser() { UserName = profileR.Username, Email = profileR.Email }, profileR.Password);

            if (!newP.Succeeded)
            {
                return BadRequest(newP.Errors);
            }

            return Created("congrats", newP);
            // var newProfile = new Profile(profileR);
            // _context.Profiles.Add(newProfile);
            // await _context.SaveChangesAsync();           
            // return Ok(newProfile);
        }
        
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(ProfileLoginDto profileL)
        {
            var profileFound = await _userManager.FindByNameAsync(profileL.Username);

            if (profileFound == null)
            {
                return NotFound();
            }

            var result = await _userManager.CheckPasswordAsync(profileFound, profileL.Password);
            if (result)
            {
                var token = _jwtService.CreateToken(profileFound);
                return Ok(new {token = token, user = profileFound });
            }

            return Unauthorized(new {mesasge = "sorry!", profileFound});
        }
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Profile>>> GetProfiles()
        {
          if (_context.Profiles == null)
          {
              return NotFound();
          }
            return await _context.Profiles.Include(p => p.Worksheets).ToListAsync();
        }

        // GET: api/Profile/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Profile>> GetProfile(long id)
        {
          if (_context.Profiles == null)
          {
              return NotFound();
          }
            var profile = await _context.Profiles.FindAsync(id);

            if (profile == null)
            {
                return NotFound();
            }

            return profile;
        }

        // PUT: api/Profile/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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
          if (_context.Profiles == null)
          {
              return Problem("Entity set 'TodoContext.Profiles'  is null.");
          }
            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProfile", new { id = profile.Id }, profile);
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
