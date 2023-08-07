using LearnApi.Models;
using LearnApi.Repositories;
using LearnApi.Services;
using Microsoft.EntityFrameworkCore;

namespace PostyTest 
{
    public class UnitTest1 : IClassFixture<TestDatabaseFixture>
    {
        private readonly PostyContext context;
        private readonly ProfileRepository profileRepository;
        private readonly AuthService authService;

        public UnitTest1()
        {
            var optionsBuilder = new DbContextOptionsBuilder<PostyContext>()
                 .UseInMemoryDatabase("PostyTest");
            context = new PostyContext(optionsBuilder.Options);
             
            profileRepository = new ProfileRepository(context);
            authService = new AuthService(profileRepository);            
        }

        [Fact]
        public async void RegisterUser_ShouldSucceed()
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            
            var newUser = new RegisterDto
            {
                Username = "string",
                Password = "password",
                Email = "string@emailcom",
            };

            var result = await authService.Register(newUser);
            Assert.True(result.Success);
        }
 
        [Fact]
        public async void RegisterExistingUser_ShouldFail()
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            
            var newUser = new RegisterDto
            {
                Username = "string",
                Password = "password",
                Email = "string@emailcom",
            };

            var result = await authService.Register(newUser);
            var result2 = await authService.Register(newUser);
            Assert.False(result2.Success);
        }       
    }
}
