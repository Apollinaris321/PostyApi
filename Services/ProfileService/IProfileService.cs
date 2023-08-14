using LearnApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace LearnApi.Services;

public interface IProfileService
{ 
    Task <ServiceResponse<ICollection<ProfileDto>>> GetAll(int pageNumber = 1, int pageSize = 10);
    Task <ServiceResponse<ProfileDto>> Get(long id);
    Task <ServiceResponse<ProfileDto>> GetByUsername(string username);
    Task <ServiceResponse<ProfileDto>> Update(ProfileDto newProfile, string sessionId);
    Task <ServiceResponse<Boolean>> Delete(long id, string sessionId);
}