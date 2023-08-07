using LearnApi.Models;
using LearnApi.Repositories;

namespace LearnApi.Services;

public class AuthService : IAuthService
{
    private readonly IProfileRepository _profileRepository;

    public AuthService(IProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<ServiceResponse<ProfileSessionDto>> Login(LoginDto loginDto)
    {
        var response = new ServiceResponse<ProfileSessionDto>();
        try
        {
            Console.WriteLine("Logindot: " + loginDto.Username + ", " + loginDto.Password);
            var profile = await _profileRepository.GetByUsername(loginDto.Username);
            if (profile == null)
            {
                response.Success = false;
                response.Message = "User doesn't exist!";
                return response;
            }
            else if (profile.SessionId != "")
            {
                 response.Success = false;
                 response.Message = "Already logged in!";
                 return response;               
            }
            else
            {
                Console.WriteLine("password: " + profile.Id + profile.Email);
                var passwordCorrect = BCrypt.Net.BCrypt.Verify(loginDto.Password, profile.PasswordHash);
                if (passwordCorrect)
                {
                    response.Success = true;
                    var sessionId = Guid.NewGuid().ToString();
                    profile.SessionId = sessionId;
                    await _profileRepository.Update(profile);
                    response.Data = new ProfileSessionDto(profile);
                    return response;
                }
                else
                {
                    response.Success = false;
                    response.Message = "Wrong password!";
                    return response;
                }
            }
        }
        catch (Exception e)
        {
            response.Success = false;
            response.Error = e.Message;
            return response;
        }
    }

    public async Task<ServiceResponse<bool>> Logout(string sessionId)
    {
         var response = new ServiceResponse<bool>();
         try
         {

             var profile = await _profileRepository.GetBySessionId(sessionId);
             if (sessionId == "")
             {
                  response.Success = false;
                  response.Message = "Already logged out!";
                  return response;                
             }
             else if (profile == null)
             {
                 response.Success = false;
                 response.Message = "Session doesn't exist!";
                 return response;
             }
             else
             {
                 profile.SessionId = "";
                 await _profileRepository.Update(profile); 
                 response.Success = true;
                 return response;
             }
         }
         catch (Exception e)
         {
             response.Success = false;
             response.Error = e.Message;
             return response;
         }       
    }

    public async Task<ServiceResponse<ProfileSessionDto>> Register(RegisterDto registerDto)
    {
        var response = new ServiceResponse<ProfileSessionDto>();
        
        var existingProfile = await _profileRepository.GetByUsername(registerDto.Username);
        if (existingProfile != null)
        {
             response.Success = false;
             response.Error = "Username taken!";
             return response;           
        }
        
        var password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
        var sessionId = Guid.NewGuid().ToString();
            
        var profile = new Profile
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = password,
            SessionId = sessionId
        };
        try
        {
            profile = await _profileRepository.Insert(profile);
            var profileDto = new ProfileSessionDto(profile);
            response.Data = profileDto;
            response.Success = true;
            return response;
        }
        catch (Exception e)
        {
             response.Data = null;
             response.Success = false;
             response.Message = e.Message;
             return response;           
        }
    }
}