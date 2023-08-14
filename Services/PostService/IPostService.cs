using LearnApi.Models;

namespace LearnApi.Services;

public interface IPostService
{
    public Task<ServiceResponse<CommentDto>> AddComment(long postId, CreateCommentDto commentDto, string sessionId);
    public Task<ServiceResponse<bool>> Like(long postId, string sessionId);
    public Task<ServiceResponse<bool>> Dislike(long postId, string sessionId);
    public Task<ServiceResponse<IEnumerable<PostDto>>> GetAll(int pageNumber = 1, int pageSize = 10);
    public Task<ServiceResponse<IEnumerable<PostDto>>> GetByUsername( string username, int pageNumber = 1, int pageSize = 10);
    public Task<ServiceResponse<PostDto>> GetById(long id);
    public Task<ServiceResponse<PostDto>> Add(CreatePostDto newPost, string sessionId);
    public Task<ServiceResponse<PostDto>> Update(long postId, CreatePostDto post, string sessionId);
    public Task<ServiceResponse<bool>> Delete(long id, string sessionId);
}