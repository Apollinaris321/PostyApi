using LearnApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LearnApi.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly PostyContext _context;

    public ProfileRepository(
         PostyContext context
        )
    {
        _context = context;
    }

    public Task<Profile> Update(Profile newProfile)
    {
        throw new NotImplementedException();
    }

    public async Task<int> CountAllProfiles()
    { 
        return await _context.Profiles
            .CountAsync();
    }
    
    public async Task<IEnumerable<Profile>> GetAllProfiles(int take, int skip)
    { 
        return await _context.Profiles
            .OrderBy(p => p.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public Task<Profile> Get(string username)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Delete(string username)
    {
        throw new NotImplementedException();
    }
}