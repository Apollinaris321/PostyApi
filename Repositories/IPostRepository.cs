using LearnApi.Models;

namespace LearnApi.Repositories;

public interface IPostRepository
{
     public Task<Post> Insert(Post post);
     public Task<Post> Update(Post post);
     public Task<Boolean> Delete(long id);
     public Task<Post?> Get(long id);
     public void Like(long postId, long profileId);
     public void Dislike(long postId, long profileId);
     public Task<ICollection<Post>> GetByUsername(string username,int offset, int take);   
     public int GetByUsernameLength(string username);   
     public Task<ICollection<Post>> GetAll(int offset, int take);   
     public int GetAllLength();   
}