using LearnApi.Models;

namespace LearnApi.Repositories;

public interface IProfileRepository
{
    public Task<Profile> Insert(Profile profile);
    public Task<Profile> Update(ProfileDto profile);
    public Task<Profile> Update(Profile profile);
    public Task<Boolean> Delete(long id);
    public Task<Profile?> Get(long id);
    public Task<Profile?> GetByUsername(string username);
    public Task<Profile?> GetBySessionId(string sessionId);
    public Task<ICollection<Profile>> GetAll(int offset, int take);
    public int GetAllLength();
}