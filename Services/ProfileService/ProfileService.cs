using System.Collections;
using LearnApi.Models;
using LearnApi.Repositories;
using LearnApi.Utils;
using Microsoft.AspNetCore.Mvc;

namespace LearnApi.Services;

public class ProfileService : IProfileService
{
    private readonly IProfileRepository _profileRepository;
    private readonly IPostRepository _postRepository;
    private readonly ISessionValidator _sessionValidator;

    public ProfileService(
        IProfileRepository profileRepository,
        IPostRepository postRepository,
        ISessionValidator sessionValidator
        )
    {
        _profileRepository = profileRepository;
        _postRepository = postRepository;
        _sessionValidator = sessionValidator;
    }

    public async Task<ServiceResponse<ICollection<ProfileDto>>> GetAll(int pageNumber = 1, int pageSize = 10)
    {
        ServiceResponse<ICollection<ProfileDto>> _response = new ServiceResponse<ICollection<ProfileDto>>();
        var len = _profileRepository.GetAllLength();
        var validFilter = new PaginationFilter(pageSize, len);
        validFilter.SetCurrentPage(pageNumber);
                              
        try
        {
            var profileList = await _profileRepository.GetAll(validFilter.Offset, validFilter.PageSize);
            var profileDtoList = profileList
                .Select(p => new ProfileDto(p))
                .ToList();
            _response.Data = profileDtoList;
            _response.LastPage = validFilter.LastPage;
            _response.CurrentPage = validFilter.CurrentPage;
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

    public async Task<ServiceResponse<ProfileDto>> GetByUsername(string username)
    {
        ServiceResponse<ProfileDto> response = new ServiceResponse<ProfileDto>();
        try
        {
            var profile = await _profileRepository.GetByUsername(username);
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

    public async Task<ServiceResponse<ProfileDto>> Update(ProfileDto newProfile,string sessionId)
    {
        var response = new ServiceResponse<ProfileDto>();
        var profile = await _sessionValidator.ValidateSession(sessionId);
        try
        {
            if (profile == null)
            {
                response.Success = false;
                response.Error = "Invalid session!";
                return response;               
            }           
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

    public async Task<ServiceResponse<bool>> Delete(long id, string sessionId)
    {
        var response = new ServiceResponse<Boolean>();
        var profile = await _sessionValidator.ValidateSession(sessionId);
        try
        {
            if (profile == null)
            {
                response.Success = false;
                response.Error = "Invalid session!";
                return response;               
            }
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