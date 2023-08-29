using LearnApi.Models;

namespace LearnApi.Services;

public interface IProfileService
{
    public Task<IServiceResponse<IEnumerable<ProfileDto>>> GetAllProfiles(int pageNumber, int pageSize);
    public Task<IServiceResponse<ProfileDto>> GetProfile(string username);
    public Task<IServiceResponse<bool>> DeleteProfile(string username);
    public Task<IServiceResponse<IEnumerable<CommentDto>>> GetProfileComments(string username, int pageNumber = 1, int pageSize = 10);
    public Task<IServiceResponse<IEnumerable<PostDto>>> GetProfilePosts(string username, int pageNumber = 1, int pageSize = 10); 

}