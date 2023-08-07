using LearnApi.Models;

namespace LearnApi.Services;

public interface IAuthService
{
     public Task<ServiceResponse<ProfileSessionDto>> Login(LoginDto loginDto);
     public Task<ServiceResponse<bool>> Logout(string sessionId);
     public Task<ServiceResponse<ProfileSessionDto>> Register(RegisterDto registerDto);
}