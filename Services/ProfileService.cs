using LearnApi.Models;
using LearnApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LearnApi.Services;

public class ProfileService : IProfileService
{
    private readonly PostyContext _context;
    private readonly IProfileRepository _profileRepository;

    public ProfileService(
            PostyContext context,
            IProfileRepository profileRepository
        )
    {
        _context = context;
        _profileRepository = profileRepository;
    }

    public async Task<IServiceResponse<IEnumerable<ProfileDto>>> GetAllProfiles(int pageNumber, int pageSize)
    {
        var response = new ServiceResponse<IEnumerable<ProfileDto>>();

        try
        {
            var len = await _profileRepository.CountAllProfiles();
            var pagination = new PaginationFilter(pageNumber, pageSize, len); 
           
            var comments = await _profileRepository
                .GetAllProfiles(pagination.PageSize, pagination.Skip());
            response.Data = comments.Select(p => new ProfileDto(p));
            response.Success = true;
            return response;

        }
        catch(Exception e)
        {
             response.Success = false;
             response.Errors = new[] {e.Message};
             return response;
        }
    }

    public async Task<IServiceResponse<ProfileDto>> GetProfile(string username)
    {
        var response = new ServiceResponse<ProfileDto>();
        try
        {
            var profile = await _profileRepository.Get(username);
            response.Data = new ProfileDto(profile);
            response.Success = true;
            return response;
        }
        catch (Exception e)
        {
            response.Errors = new[] {e.Message};
            response.Success = false;
            return response;
        }
    }

    public async Task<IServiceResponse<bool>> DeleteProfile(string username)
    {
        var response = new ServiceResponse<bool>();
        try
        {
            var result = await _profileRepository.Delete(username);
            response.Data = result;
            response.Success = true;
            return response;
        }
        catch (Exception e)
        {
            response.Success = false;
            response.Errors = new[] {e.Message};
            return response;
        }
    }

    public Task<IServiceResponse<IEnumerable<CommentDto>>> GetProfileComments(string username, int pageNumber = 1, int pageSize = 10)
    {
        throw new NotImplementedException();
    }

    public Task<IServiceResponse<IEnumerable<PostDto>>> GetProfilePosts(string username, int pageNumber = 1, int pageSize = 10)
    {
        throw new NotImplementedException();
    }
}