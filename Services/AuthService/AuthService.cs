﻿using LearnApi.Models;
using LearnApi.Repositories;
using LearnApi.Utils;

namespace LearnApi.Services;

public class AuthService : IAuthService
{
    private readonly IProfileRepository _profileRepository;
    private readonly ISessionValidator _validator;

    public AuthService(
        IProfileRepository profileRepository,
        ISessionValidator validator
        )
    {
        _profileRepository = profileRepository;
        _validator = validator;
    }

    public async Task<ServiceResponse<ProfileSessionDto>> Login(LoginDto loginDto)
    {
        var response = new ServiceResponse<ProfileSessionDto>();
        try
        {
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
         var profile = await _validator.ValidateSession(sessionId);
         try
         {
             if (profile == null)
             {
                   response.Success = false;
                   response.Message = "No profile found!";
                   return response;                                
             }
             if (profile.SessionId == "")
             {
                  response.Success = false;
                  response.Message = "Already logged out!";
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