using LearnApi.Models;
using LearnApi.Repositories;
using LearnApi.Services;
using LearnApi.Utils;
using Microsoft.EntityFrameworkCore;

namespace PostyTest 
{
    public class AuthTest : IClassFixture<TestDatabaseFixture>
    {
        private readonly PostyContext context;
        private readonly ProfileRepository profileRepository;
        private readonly AuthService authService;
        private readonly SessionValidator sessionValidator;
        private readonly PostRepository postRepository;
        private readonly PostService postService;

        public AuthTest()
        {
            var optionsBuilder = new DbContextOptionsBuilder<PostyContext>()
                 .UseInMemoryDatabase("AuthTest");
            context = new PostyContext(optionsBuilder.Options);
             
            profileRepository = new ProfileRepository(context);
            sessionValidator = new SessionValidator(profileRepository);
            authService = new AuthService(profileRepository, sessionValidator);
            postRepository = new PostRepository(context);
            postService = new PostService(postRepository, profileRepository, sessionValidator);
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
