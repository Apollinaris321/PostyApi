using LearnApi.Models;

namespace LearnApi.Repositories;

public interface IProfileRepository
{

    public Task<IEnumerable<Profile>> GetAllProfiles(int take, int skip);
    public Task<Profile> Get(string username);
    public Task<bool> Delete(string username);
    public Task<Profile> Update(Profile newProfile);
    public Task<int> CountAllProfiles();
}