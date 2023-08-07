using LearnApi.Models;
using LearnApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearnApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(
        IAuthService authService
        )
    {
        _authService = authService;
    }
 
    [HttpPost]
    [Route("register")]
    public async Task<ActionResult<ProfileSessionDto>> Register(RegisterDto registerDto)
    {
        var response = await _authService.Register(registerDto);
        if (response.Success)
        {
            HttpContext.Response.Cookies.Append("Auth", response.Data.SessionId);
            return Ok(response.Data);
        }
        else
        {
            return BadRequest(response.Error);
        }
    }   
         
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var response = await _authService.Login(loginDto);
        if (response.Success)
        {
            HttpContext.Response.Cookies.Append("Auth", response.Data.SessionId);
            return Ok(response.Data);
        }
        else
        {
            return BadRequest(response.Error + ", "+ response.Message);
        }       
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var sessionId = HttpContext.Request.Cookies["Auth"];
        if (sessionId == null)
        {
            return BadRequest("no cookie present!");
        }
        
        var response = await _authService.Logout(sessionId);
        if (response.Success)
        {
            HttpContext.Response.Cookies.Delete("Auth");
            return Ok();
        }
        else
        {
            return BadRequest("message: " + response.Message + ", error: " + response.Error);
        }       
    }   
}