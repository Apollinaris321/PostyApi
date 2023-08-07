using LearnApi.Models;

namespace LearnApi.Services;

public interface IPostService
{
    public ServiceResponse<CommentDto> AddComment(long postId, CommentDto commentDto, string sessionId);
    public Task<ServiceResponse<bool>> Like(long postId, string sessionId);
    public Task<ServiceResponse<bool>> Dislike(long postId, string sessionId);
    public Task<ServiceResponse<IEnumerable<PostDto>>> GetAll(int pageNumber = 10, int pageSize = 10);
    public Task<ServiceResponse<IEnumerable<PostDto>>> GetByUsername(string username, int pageNumber = 10, int pageSize = 10);
    public Task<ServiceResponse<PostDto>> GetById(long id);
    public Task<ServiceResponse<PostDto>> Add(CreatePostDto newPost, string sessionId);
    public Task<ServiceResponse<PostDto>> Update(PostDto post, string sessionId);
    public Task<ServiceResponse<bool>> Delete(long id, string sessionId);
}