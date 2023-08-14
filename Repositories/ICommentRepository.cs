using LearnApi.Models;

namespace LearnApi.Repositories;

public interface ICommentRepository
{
      public Task<Comment> Insert(Comment comment);
      public Task<Comment> Update(Comment comment);
      public Task<Boolean> Delete(long id);
      public Task<Comment?> Get(long id);
      public void Like(long postId, long profileId);
      public void Dislike(long postId, long profileId);
      public Task<ICollection<Comment>> GetByUsername(string username,int offset, int take);   
      public int GetByUsernameLength(string username);   
      public Task<ICollection<Comment>> GetAll(int offset, int take);   
      public Task<ICollection<Comment>> GetByPostId(long postId, int offset, int take);   
      public int GetByPostIdLength(long postId);   
      public Task<ICollection<Comment>> GetByProfileId(long profileId, int offset, int take);   
      public int GetByProfileIdLength(long profileId);   
      public int GetAllLength();      
}