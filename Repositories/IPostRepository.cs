using LearnApi.Models;

public interface IPostRepository
{
    public Task<IEnumerable<Post>> GetByUsername(string username, int take, int offset);
}