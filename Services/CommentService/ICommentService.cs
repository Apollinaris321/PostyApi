using LearnApi.Models;

namespace LearnApi.Services;

public interface ICommentService
{
     public Task<ServiceResponse<bool>> Like(long commentId, string sessionId);
     public Task<ServiceResponse<bool>> Dislike(long commentId, string sessionId);
     public Task<ServiceResponse<IEnumerable<CommentDto>>> GetAll(int pageNumber = 1, int pageSize = 10);
     public Task<ServiceResponse<CommentDto>> GetById(long id);
     public Task<ServiceResponse<IEnumerable<CommentDto>>> GetByPostId(long id, int offSet, int pageSize);
     public Task<ServiceResponse<IEnumerable<CommentDto>>> GetByUsername(string username, int offSet, int pageSize);
     public Task<ServiceResponse<CommentDto>> Add(long postId, CreateCommentDto newComment, string sessionId);
     public Task<ServiceResponse<CommentDto>> Update(CreateCommentDto comment, long commentId , string sessionId);
     public Task<ServiceResponse<bool>> Delete(long id, string sessionId);   
}