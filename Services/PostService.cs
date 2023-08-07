using System.Collections;
using AutoMapper;
using LearnApi.Models;
using LearnApi.Repositories;

namespace LearnApi.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IMapper _mapper;

    public PostService(
        IPostRepository postRepository,
        IProfileRepository profileRepository,
        IMapper mapper
        )
    {
        _postRepository = postRepository;
        _profileRepository = profileRepository;
        _mapper = mapper;
    }

    public async Task<ServiceResponse<IEnumerable<PostDto>>> GetAll(int pageNumber = 10, int pageSize = 10)
    {
        ServiceResponse<IEnumerable<PostDto>> response = new ServiceResponse<IEnumerable<PostDto>>();

        try
        { 
            var len = _postRepository.GetAllLength();
            var validFilter = new PaginationFilter(pageSize, len);
            validFilter.SetCurrentPage(pageNumber);           
            
            var posts = await _postRepository.GetAll();
            var postDtos = posts.Select(post => new PostDto(post));

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

    public async Task<ServiceResponse<IEnumerable<PostDto>>> GetByUsername(string username, int pageNumber = 10, int pageSize = 10)
    {
        ServiceResponse<IEnumerable<PostDto>> response = new ServiceResponse<IEnumerable<PostDto>>();

        try
        { 
            var len = _postRepository.GetByUsernameLength(username);
            var validFilter = new PaginationFilter(pageSize, len);
            validFilter.SetCurrentPage(pageNumber);           
            
            var posts = await _postRepository.GetByUsername(username);
            var postDtos = posts.Select(post => new PostDto(post));

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


    public async Task<ServiceResponse<bool>> Like(long postId, string sessionId)
    {
        var response = new ServiceResponse<bool>();

        try
        {
            var profile = await _profileRepository.GetBySessionId(sessionId);
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

        try
        {
            var profile = await _profileRepository.GetBySessionId(sessionId);
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
        try
        {
            var profile = await _profileRepository.GetBySessionId(sessionId);
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

    public async Task<ServiceResponse<PostDto>> Update(PostDto newPost, string sessionId)
    {
        var response = new ServiceResponse<PostDto>();
        try
        {
            var profile = await _profileRepository.GetBySessionId(sessionId);
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

    public async  Task<ServiceResponse<bool>> Delete(long id, string sessionId)
    {
         var response = new ServiceResponse<bool>();
         try
         {
             var profile = await _profileRepository.GetBySessionId(sessionId);
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
    
    public ServiceResponse<CommentDto> AddComment(long postId, CommentDto commentDto, string sessionId)
    {
        throw new NotImplementedException();
    }
}