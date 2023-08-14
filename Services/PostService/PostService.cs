using System.Collections;
using AutoMapper;
using LearnApi.Models;
using LearnApi.Repositories;
using LearnApi.Utils;

namespace LearnApi.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly ISessionValidator _sessionValidator;

    public PostService(
        IPostRepository postRepository,
        IProfileRepository profileRepository,
        ISessionValidator sessionValidator
        )
    {
        _postRepository = postRepository;
        _profileRepository = profileRepository;
        _sessionValidator = sessionValidator;
    }

    public async Task<ServiceResponse<IEnumerable<PostDto>>> GetAll(int pageNumber = 1, int pageSize = 10)
    {
        ServiceResponse<IEnumerable<PostDto>> response = new ServiceResponse<IEnumerable<PostDto>>();
        try
        { 
            var len = _postRepository.GetAllLength();
            var validFilter = new PaginationFilter(pageSize, len);
            validFilter.SetCurrentPage(pageNumber);           
            
            var posts = await _postRepository.GetAll(validFilter.Offset, validFilter.PageSize);
            var postDtos = posts
                .Select(post => new PostDto(post));

            response.Data = postDtos;
            response.Success = true;
            return response;
        }
        catch (Exception e)
        {
            response.Error = e.Message;
            response.Success = false;
            return response;           
        }
    }

    public async Task<ServiceResponse<IEnumerable<PostDto>>> GetByUsername(string username, int pageNumber = 1, int pageSize = 10)
    {
         ServiceResponse<IEnumerable<PostDto>> response = new ServiceResponse<IEnumerable<PostDto>>();
         try
         { 
             var len = _postRepository.GetByUsernameLength(username);
             var validFilter = new PaginationFilter(pageSize, len);
             validFilter.SetCurrentPage(pageNumber);           
             
             var posts = await _postRepository.GetByUsername(username, validFilter.Offset, validFilter.PageSize);
             var postDtos = posts
                 .Select(post => new PostDto(post));
 
             response.Data = postDtos;
             response.Success = true;
             return response;
         }
         catch (Exception e)
         {
             response.Error = e.Message;
             response.Success = false;
             return response;           
         }       
    }


    public async Task<ServiceResponse<PostDto>> GetById(long id)
    {
        var response = new ServiceResponse<PostDto>();
        try
        {
            var post = await _postRepository.Get(id);
            if (post == null)
            {
                response.Success = false;
                response.Error = "Post not found!";
                return response;
            }
            
            var postDto = new PostDto(post);
            response.Data = postDto;
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

    public async Task<ServiceResponse<PostDto>> Add(CreatePostDto newPost, string sessionId)
    {
        var response = new ServiceResponse<PostDto>();
        var profile = await _sessionValidator.ValidateSession(sessionId);
        try
        {
            if (profile == null)
            {
                response.Success = false;
                response.Error = "Profile not found!";
                return response;
            }
            
            var post = new Post
            {
                Text = newPost.Text,
                Profile = profile
                
            };
            post = await _postRepository.Insert(post);
            
            var postDto = new PostDto(post);
            response.Data = postDto;
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

    public async Task<ServiceResponse<PostDto>> Update(long postId, CreatePostDto newPost, string sessionId)
    {
        var response = new ServiceResponse<PostDto>();
        var profile = await _sessionValidator.ValidateSession(sessionId);
        try
        {
            if (profile == null)
            {
                response.Success = false;
                response.Error = "Profile not found!";
                return response;
            }
            
            var post = new Post
            {
                Id = postId,
                Text = newPost.Text,
                Profile = profile
                
            };
            post = await _postRepository.Update(post);
            
            var postDto = new PostDto(post);
            response.Data = postDto;
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

    public async  Task<ServiceResponse<bool>> Delete(long id, string sessionId)
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

             await _postRepository.Delete(id);
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
    
    public Task<ServiceResponse<CommentDto>> AddComment(long postId, CreateCommentDto commentDto, string sessionId)
    {
        throw new NotImplementedException();
    }
 
    public ServiceResponse<CommentDto> RemoveComment(long postId, CommentDto commentDto, string sessionId)
    {
        throw new NotImplementedException();
    }   
 
    public async Task<ServiceResponse<bool>> Like(long postId, string sessionId)
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
            
            _postRepository.Like(postId, profile.Id);
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

    public async Task<ServiceResponse<bool>> Dislike(long postId, string sessionId)
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
            
            _postRepository.Dislike(postId, profile.Id);
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
}