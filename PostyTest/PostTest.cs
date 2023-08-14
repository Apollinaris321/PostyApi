using LearnApi.Models;
using LearnApi.Repositories;
using LearnApi.Services;
using LearnApi.Utils;
using Microsoft.EntityFrameworkCore;

namespace PostyTest;

public class PostTest : IClassFixture<TestDatabaseFixture>
{
         private readonly PostyContext context;
         private readonly ProfileRepository profileRepository;
         private readonly AuthService authService;
         private readonly SessionValidator sessionValidator;
         private readonly PostRepository postRepository;
         private readonly PostService postService;
         private ProfileSessionDto? profile = null;

         public PostTest()
         {
             var optionsBuilder = new DbContextOptionsBuilder<PostyContext>()
                  .UseInMemoryDatabase("PostTest");
             context = new PostyContext(optionsBuilder.Options);
              
             profileRepository = new ProfileRepository(context);
             sessionValidator = new SessionValidator(profileRepository);
             authService = new AuthService(profileRepository, sessionValidator);
             postRepository = new PostRepository(context);
             postService = new PostService(postRepository, profileRepository, sessionValidator);
         }
 
         public async Task Register()
         {
             await context.Database.EnsureDeletedAsync();
             await context.Database.EnsureCreatedAsync();
                         
              var newUser = new RegisterDto
              {
                  Username = "string",
                  Password = "password",
                  Email = "string@emailcom",
              };
  
              var response = await authService.Register(newUser);
              profile = response.Data;           
         }
        
        [Fact]
        public async void AddPost_ShouldSucceed()
        {
            await Register();
            var newPost = new CreatePostDto
            {
                Text = "hello",
                Title = "first time"
            };

            var postResult = await postService.Add(newPost, profile.SessionId);
            Assert.True(postResult.Success);
            Assert.NotNull(postResult.Data);
            Assert.Equal("hello", postResult.Data.Text);
        }
          
        [Fact]
        public async void DeletePost_ShouldSucceed()
        {
            await Register();
            
            var newPost = new CreatePostDto
            {
                Text = "hello",
                Title = "first time"
            };

            var postResult = await postService.Add(newPost, profile.SessionId);
            var removePostResult = await postService.Delete(postResult.Data.Id, profile.SessionId);
            var findPost = await postService.GetById(postResult.Data.Id);
            Assert.False(findPost.Success);
        }
           
        [Fact]
        public async void LikePost_ShouldSucceed()
        {
            await Register();
            
            var newPost = new CreatePostDto
            {
                Text = "hello",
                Title = "first time"
            };

            var postResult = await postService.Add(newPost, profile.SessionId);
            var likePostResult = await postService.Like(postResult.Data.Id, profile.SessionId);
            var findPost = await postService.GetById(postResult.Data.Id);
            Assert.Equal(1, findPost.Data.Likes);
        }       
        
        [Fact]
        public async void DislikePost_ShouldSucceed()
        {
         
            await Register();
            
            var newPost = new CreatePostDto
            {
                Text = "hello",
                Title = "first time"
            };

            var postResult = await postService.Add(newPost, profile.SessionId);
            var likePostResult = await postService.Like(postResult.Data.Id, profile.SessionId);
            var findPost = await postService.GetById(postResult.Data.Id);
            Assert.Equal(1, findPost.Data.Likes);
            
            var dislikePostResult = await postService.Dislike(postResult.Data.Id, profile.SessionId);
            findPost = await postService.GetById(postResult.Data.Id);
            Assert.Equal(0, findPost.Data.Likes);
        }       
       
}           
