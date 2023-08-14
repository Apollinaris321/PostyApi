using System.Collections;
using LearnApi.Models;
using LearnApi.Repositories;
using LearnApi.Utils;

namespace LearnApi.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly ISessionValidator _sessionValidator;

    public CommentService(
        ICommentRepository commentRepository,
        IPostRepository postRepository,
        IProfileRepository profileRepository,
        ISessionValidator sessionValidator       
        )
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
        _profileRepository = profileRepository;
        _sessionValidator = sessionValidator;
    }

    public async Task<ServiceResponse<bool>> Like(long commentId, string sessionId)
    {
        var response = new ServiceResponse<bool>();
        var profile = await _sessionValidator.ValidateSession(sessionId);
        try
        {
            if (profile == null)
            {
                response.Success = false;
                response.Error = "User not found!";
                return response;
            }
            
            _commentRepository.Like(commentId, profile.Id);
            response.Success = true;
            return response;
        }
        catch (Exception e)
        {
            response.Success = false;
            response.Error = "Failed to like post!";           
            return response;
        }
    }

    public async Task<ServiceResponse<bool>> Dislike(long commentId, string sessionId)
    {
        var response = new ServiceResponse<bool>();
        var profile = await _sessionValidator.ValidateSession(sessionId);
        try
        {
            if (profile == null)
            {
                response.Success = false;
                response.Error = "User not found!";
                return response;
            }
            
            _commentRepository.Dislike(commentId, profile.Id);
            response.Success = true;
            return response;
        }
        catch (Exception e)
        {
            response.Success = false;
            response.Error = "Failed to like post!";           
            return response;
        }
    }

    public async Task<ServiceResponse<IEnumerable<CommentDto>>> GetAll(int pageNumber = 1, int pageSize = 10)
    {
        var response = new ServiceResponse<IEnumerable<CommentDto>>();
        try
        {
            var len = _commentRepository.GetAllLength();
            var validFilter = new PaginationFilter(pageSize, len);
            validFilter.SetCurrentPage(pageNumber);           
            
            var comments = await _commentRepository.GetAll(validFilter.Offset, validFilter.PageSize);
            var commentDtos = comments
                .Select(comment => new CommentDto(comment));

            response.Data = commentDtos;
            response.Success = true;
            return response;
        }
        catch (Exception e)
        {
            response.Success = false;
            response.Error = e.Message;
            return response;
        }
    }

    public async Task<ServiceResponse<CommentDto>> GetById(long id)
    {
        var response = new ServiceResponse<CommentDto>();
        try
        {
            var comment = await _commentRepository.Get(id);
            if (comment == null)
            {
                response.Error = "Comment not found!";
                response.Success = false;
                return response;               
            }
            var commentDto = new CommentDto(comment);
            response.Data = commentDto;
            response.Success = true;
            return response;
        }
        catch (Exception e)
        {
            response.Success = false;
            response.Error = e.Message;
            return response;
        }
    }

    public async Task<ServiceResponse<IEnumerable<CommentDto>>> GetByPostId(long postId, int pageNumber = 1, int pageSize = 10)
    {
        var len = _commentRepository.GetByPostIdLength(postId);
        var validFilter = new PaginationFilter(pageSize, len);
        validFilter.SetCurrentPage(pageNumber);           
                    
        var response = new ServiceResponse<IEnumerable<CommentDto>>();
        try
        {
            var comments = await _commentRepository.GetByPostId(postId, validFilter.Offset, validFilter.PageSize);
            var commentDtos = comments
                .Select(c => new CommentDto(c));
            response.Data = commentDtos;
            response.Success = true;
            return response;
        }
        catch (Exception e)
        {
            response.Success = false;
            response.Error = e.Message;
            return response;
        }
    }

    public async Task<ServiceResponse<IEnumerable<CommentDto>>> GetByUsername(string username, int pageNumber, int pageSize)
    {
        var len = _commentRepository.GetByUsernameLength(username);
        var validFilter = new PaginationFilter(pageSize, len);
        validFilter.SetCurrentPage(pageNumber);           
                    
        var response = new ServiceResponse<IEnumerable<CommentDto>>();
        try
        {
            var comments = await _commentRepository.GetByUsername(username, validFilter.Offset, validFilter.PageSize);
            var commentDtos = comments
                .Select(c => new CommentDto(c));
            response.Data = commentDtos;
            response.Success = true;
            return response;
        }
        catch (Exception e)
        {
            response.Success = false;
            response.Error = e.Message;
            return response;
        }
    }

    public async Task<ServiceResponse<CommentDto>> Add(long postId, CreateCommentDto newComment, string sessionId)
    {
        var response = new ServiceResponse<CommentDto>();
        var profile = await _sessionValidator.ValidateSession(sessionId);
        try
        {
            if (profile == null)
            {
                response.Success = false;
                response.Error = "Profile not found!";
                return response;
            }

            var post = await _postRepository.Get(postId);

            if (post == null)
            {
                 response.Success = false;
                 response.Error = "Post not found!";
                 return response;               
            }
            
            var comment = new Comment
            {
                Text = newComment.Text,
                Profile = profile,
                Post = post
            };
            
            comment = await _commentRepository.Insert(comment);
            
            var commentDto = new CommentDto(comment);
            response.Data = commentDto;
            response.Success = true;
            return response;
        }
        catch (Exception e)
        {
            response.Success = false;
            response.Error = e.Message;
            return response;
        }
    }

    public async Task<ServiceResponse<CommentDto>> Update(CreateCommentDto newComment, long commentId, string sessionId)
    {
        var response = new ServiceResponse<CommentDto>();
        var profile = await _sessionValidator.ValidateSession(sessionId);
        try
        {
            if (profile == null)
            {
                response.Success = false;
                response.Error = "Profile not found!";
                return response;
            }
            
            var comment = new Comment
            {
                Id = commentId,
                Text = newComment.Text,
                Profile = profile
            };
            
            comment = await _commentRepository.Update(comment);
            
            var commentDto = new CommentDto(comment);
            response.Data = commentDto;
            response.Success = true;
            return response;
        }
        catch (Exception e)
        {
            response.Success = false;
            response.Error = e.Message;
            return response;
        }
    }

    public async Task<ServiceResponse<bool>> Delete(long id, string sessionId)
    {
         var response = new ServiceResponse<bool>();
         var profile = await _sessionValidator.ValidateSession(sessionId);
         try
         {
             if (profile == null)
             {
                 response.Success = false;
                 response.Error = "Profile not found!";
                 return response;
             }
             
             await _commentRepository.Delete(id);
             response.Success = true;
             return response;
         }
         catch (Exception e)
         {
             response.Success = false;
             response.Error = e.Message;
             return response;
         }       
    }
}