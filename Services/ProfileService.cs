using LearnApi.Models;
using LearnApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace LearnApi.Services;

public class ProfileService : IProfileService
{
    private readonly IProfileRepository _profileRepository;
    private readonly IPostRepository _postRepository;

    public ProfileService(
        IProfileRepository profileRepository,
        IPostRepository postRepository
        )
    {
        _profileRepository = profileRepository;
        _postRepository = postRepository;
    }


    public Task<IActionResult> GetComments(string username, int pageNumber = 1, int pageSize = 10)
    { 
        throw new NotImplementedException();
    }

    public Task<IActionResult> GetPosts(string username, int pageNumber = 1, int pageSize = 10)
    {
        throw new NotImplementedException();
    }
    
    public async Task<ServiceResponse<ICollection<ProfileDto>>> GetAll()
    {
        ServiceResponse<ICollection<ProfileDto>> _response = new ServiceResponse<ICollection<ProfileDto>>();
        try
        {
            var profileList = await _profileRepository.GetAll();
            var profileDtoList = profileList
                .Select(p => new ProfileDto(p))
                .ToList();
            _response.Data = profileDtoList;
            _response.Success = true;
            return _response;
        }
        catch (Exception ex)
        {
            _response.Success = false;
            return _response;
        }
        return _response;
    }

    public async Task<ServiceResponse<ProfileDto>> Get(long id)
    {
        ServiceResponse<ProfileDto> response = new ServiceResponse<ProfileDto>();
        try
        {
            var profile = await _profileRepository.Get(id);
            if (profile == null)
            {
                response.Success = false;
                response.Data = null;
                response.Message = "Profile not found";
                return response;
            }

            var profileDto = new ProfileDto(profile);
            response.Data = profileDto;
            response.Success = true;
            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            return response;
        }
        return response;
    }

    public async Task<ServiceResponse<ProfileDto>> Add(ProfileDto newProfile)
    {
        var response = new ServiceResponse<ProfileDto>();
        var profile = new Profile
        {
            Username = newProfile.Username,
            Email = newProfile.Email
        };

        try
        {
            var savedProfile = await _profileRepository.Insert(profile);
            var profileDto = new ProfileDto(savedProfile);
            response.Data = profileDto;
            response.Success = true;
            return response;
        }
        catch (Exception e)
        {
            response.Error = e.Message;
            response.Success = false;
            return response;
        }
        return response;
    }


    public async Task<ServiceResponse<ProfileDto>> Update(ProfileDto newProfile)
    {
        var response = new ServiceResponse<ProfileDto>();
        try
        {
            var savedProfile = await _profileRepository.Update(newProfile);
            var profileDto = new ProfileDto(savedProfile);
            response.Data = profileDto;
            response.Success = true;
            return response;
        }
        catch (Exception e)
        {
            response.Error = e.Message;
            response.Success = false;
            return response;
        }
        return response;
    }

    public async Task<ServiceResponse<bool>> Delete(long id)
    {
        var response = new ServiceResponse<Boolean>();
        try
        {
            var result = await _profileRepository.Delete(id);
            response.Success = result;
            return response;
        }
        catch (Exception e)
        {
            response.Success = false;
            response.Error = e.Message;
            return response;
        }
        return response;
    }
}