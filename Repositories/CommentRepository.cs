using LearnApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LearnApi.Repositories;

public class CommentRepository : ICommentRepository
{
     private readonly PostyContext _context;
 
     public CommentRepository(PostyContext _context)
     {
         this._context = _context;
     }
 
     public async Task<Comment> Insert(Comment comment)
     {
         var addedComment = await _context.Comments.AddAsync(comment);
         await _context.SaveChangesAsync();
         return addedComment.Entity;
     }
 
     public async Task<Comment> Update(Comment comment)
     {
         var oldComment = await _context.Comments
             .SingleOrDefaultAsync(c => c.Id == comment.Id);
         
         if (oldComment == null)
         {
             throw new Exception("comment does not exist!");
         }
         
         _context.Entry(oldComment).CurrentValues.SetValues(comment);
         await _context.SaveChangesAsync();
         return comment;
     }
 
     public async Task<bool> Delete(long id)
     {
         var comment = await _context.Comments
             .Include(c => c.Profile)
             .Include(c => c.Post)
             .Include(c => c.LikedBy)
             .SingleOrDefaultAsync(c => c.Id == id);
         
         if (comment == null)
         {
             return false;
         }
         _context.Comments.Remove(comment);
         await _context.SaveChangesAsync();
         return true;
     }
 
     public async Task<Comment?> Get(long id)
     {
         return await _context.Comments
             .Include(c => c.Profile)
             .Include(c => c.LikedBy)
             .SingleOrDefaultAsync(c => c.Id == id);
     }
     
     public async Task<ICollection<Comment>> GetByUsername(string username, int offset = 0, int take = 10)
     {
         return await _context.Comments
                 .Include(c => c.Profile)
                 .Include(c => c.LikedBy)
                 .Where(c => c.Profile.Username == username)
                 .OrderByDescending(c => c.CreatedAt)
                 .Skip(offset)
                 .Take(take)
                 .ToListAsync();                   
     }
 
     public int GetByUsernameLength(string username)
     {
         return _context.Comments
             .Count(c => c.Profile.Username == username);
     }
     
     public async Task<ICollection<Comment>> GetAll(int offset = 1, int take = 10)
     {
         return await _context.Comments
                 .Include(c => c.Profile)
                 .Include(c => c.LikedBy)
                 .OrderByDescending(c => c.CreatedAt)
                 .Skip(offset)
                 .Take(take)
                 .ToListAsync();    
     }

     public async Task<ICollection<Comment>> GetByPostId(long postId, int offset, int take)
     {
         return await _context.Comments
             .Where(c => c.PostId == postId)
             .Include(c => c.Profile)
             .Include(c => c.LikedBy)
             .OrderByDescending(c => c.CreatedAt)
             .Skip(offset)
             .Take(take)
             .ToListAsync();    
     }

     public int GetByPostIdLength(long postId)
     {
         return _context.Comments
             .Count(c => c.PostId == postId);
     }

     public async Task<ICollection<Comment>> GetByProfileId(long profileId, int offset, int take)
     {
         return await _context.Comments
             .Where(c => c.ProfileId == profileId)
             .Include(c => c.Profile)
             .Include(c => c.LikedBy)
             .OrderByDescending(c => c.CreatedAt)
             .Skip(offset)
             .Take(take)
             .ToListAsync();
     }

     public int GetByProfileIdLength(long profileId)
     {
         return _context.Comments
             .Count(c => c.ProfileId == profileId);
     }

     public int GetAllLength()
     {
         return _context.Comments
             .Count();
     }
     
     public async void Like(long commentId, long profileId)
     {
         var like = new CommentLike
         {
             CommentId = commentId,
             ProfileId = profileId
         };
 
         _context.CommentLikes.Add(like);
         await _context.SaveChangesAsync();
     }
 
     public async void Dislike(long commentId, long profileId)
     {
         var like = await _context.CommentLikes
             .SingleOrDefaultAsync(c => c.CommentId == commentId && c.ProfileId == profileId);
         _context.CommentLikes.Remove(like);
         await _context.SaveChangesAsync();
     }
}