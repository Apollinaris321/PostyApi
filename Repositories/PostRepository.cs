using LearnApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LearnApi.Repositories;

public class PostRepository : IPostRepository
{
    private readonly PostyContext _context;

    public PostRepository(PostyContext _context)
    {
        this._context = _context;
    }

    public async Task<Post> Insert(Post post)
    {
        var addedPost = await _context.Posts.AddAsync(post);
        return addedPost.Entity;
    }

    public async Task<Post> Update(Post post)
    {
        var oldPost = await _context.Posts
            .SingleOrDefaultAsync(p => p.Id == post.Id);
        
        if (oldPost == null)
        {
            throw new Exception("post does not exist!");
        }
        
        _context.Entry(oldPost).CurrentValues.SetValues(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task<bool> Delete(long id)
    {
        var post = await _context.Posts
            .Include(p => p.Profile)
            .Include(p => p.Comments)
            .Include(p => p.ProfileLikes)
            .SingleOrDefaultAsync(p => p.Id == id);
        
        if (post == null)
        {
            return false;
        }
        _context.Posts.Remove(post);
        return true;
    }


    public async Task<ICollection<Post>> GetByUsername(string username, int pageSize = 10, int pageNumber = 1)
    {
        return await _context.Posts
                .Include(post => post.Profile)
                .Include(post => post.ProfileLikes)
                .Where(post => post.Profile.Username == username)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();                   
    }

    public int GetByUsernameLength(string username)
    {
        return _context.Posts
            .Count(post => post.Profile.Username == username);
    }

    public async Task<Post?> Get(long id)
    {
        return await _context.Posts
            .Include(p => p.Profile)
            .Include(p => p.ProfileLikes)
            .SingleOrDefaultAsync(p => p.Id == id);
    }

    public async void Like(long postId, long profileId)
    {
        var like = new PostLike
        {
            PostId = postId,
            ProfileId = profileId
        };

        _context.PostLikes.Add(like);
        await _context.SaveChangesAsync();
    }

    public async void Dislike(long postId, long profileId)
    {
        var like = await _context.PostLikes.SingleOrDefaultAsync(pl => pl.PostId == postId && pl.ProfileId == profileId);
        _context.PostLikes.Remove(like);
        await _context.SaveChangesAsync();
    }
    
    public async Task<ICollection<Post>> GetAll(int pageNumber = 1, int pageSize = 10)
    {
        return await _context.Posts
                .Include(post => post.Profile)
                .Include(post => post.ProfileLikes)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();    
    }

    public int GetAllLength()
    {
        return _context.Posts
            .Count();
    }
}