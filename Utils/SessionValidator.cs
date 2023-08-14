using LearnApi.Models;
using LearnApi.Repositories;

namespace LearnApi.Utils;

public class SessionValidator : ISessionValidator
{
    private readonly IProfileRepository _profileRepository;

    public SessionValidator(
            IProfileRepository profileRepository
        )
    {
        this._profileRepository = profileRepository;
    }

    public async Task<Profile?> ValidateSession(string sessionId)
    {
        if (string.IsNullOrEmpty(sessionId))
        {
            return null;
        }
        
        try
        {
            var profile = await this._profileRepository.GetBySessionId(sessionId);
            if (profile == null)
            {
                return null;
            }
            return profile;
        }
        catch (Exception e)
        {
            return null;
        }
    }
}
