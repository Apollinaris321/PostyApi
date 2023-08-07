using LearnApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace LearnApi.Services;

public interface IProfileService
{ 
    public Task<IActionResult> GetComments(string username, int pageNumber = 1, int pageSize = 10);
    public Task<IActionResult> GetPosts(string username, int pageNumber = 1, int pageSize = 10);
    Task <ServiceResponse<ProfileDto>> Update(ProfileDto newProfile);
    Task <ServiceResponse<ProfileDto>> Get(long id);
    Task <ServiceResponse<ICollection<ProfileDto>>> GetAll();
    Task <ServiceResponse<ProfileDto>> Add(RegisterDto profile);
    Task <ServiceResponse<Boolean>> Delete(long id);
}