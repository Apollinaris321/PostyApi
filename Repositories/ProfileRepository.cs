using LearnApi.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace LearnApi.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly PostyContext _context;

    public ProfileRepository(PostyContext context)
    {
        _context = context;
    }
    
    public async Task<Profile> Insert(Profile profile)
    {
        var result = await _context.Profiles.AddAsync(profile);
        await _context.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<Profile> Update(Profile newProfile)
    {
        var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.Username == newProfile.Username);
        
        if (profile == null)
        {
            throw new Exception("profile does not exist!");
        }
        
        _context.Entry(profile).CurrentValues.SetValues(newProfile);
        await _context.SaveChangesAsync();
        return profile;
    }
    
    public async Task<Profile> Update(ProfileDto newProfile)
    {
        var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.Username == newProfile.Username);
        
        if (profile == null)
        {
            throw new Exception("profile does not exist!");
        }
        
        _context.Entry(profile).CurrentValues.SetValues(newProfile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task<bool> Delete(long id)
    {
        var profile = await _context.Profiles
            .Include(p => p.Comments)
            .Include(p => p.Posts)
            .Include(p => p.LikedComments)
            .Include(p => p.LikedPosts)
            .SingleOrDefaultAsync(p => p.Id == id);
        if (profile == null)
        {
            return false;
        }
        _context.Remove(profile);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Profile?> Get(long id)
    {
         var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.Id == id);
         return profile;
    }

    public async Task<Profile?> GetByUsername(string username)
    {
        var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.Username == username);
        return profile;       
    }

    public async Task<Profile?> GetBySessionId(string sessionId)
    {
        var profile = await _context.Profiles.SingleOrDefaultAsync(p => p.SessionId == sessionId);
        return profile;       
    }

    public int GetAllLength()
    {
        return _context.Profiles.Count();
    }

    public async Task<ICollection<Profile>> GetAll(int offset, int take)
    {
        return await _context.Profiles
            .Skip(offset)
            .Take(take)
            .ToListAsync();
    }
}