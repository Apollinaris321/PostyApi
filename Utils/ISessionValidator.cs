using LearnApi.Models;

namespace LearnApi.Utils;

public interface ISessionValidator
{
    public Task<Profile?> ValidateSession(string sessionId);
}