using LearnApi.Models;
using Profile = AutoMapper.Profile;

namespace LearnApi;

public class MyMappingProfile : Profile
{
     public MyMappingProfile()
     {
         // CreateMap<TSource, TDestination>() defines the mapping between the entity and DTO
         CreateMap<Post, PostDto>();
     }    
}