using LearnApi.Models;
using LearnApi.Repositories;
using LearnApi.Services;
using LearnApi.Utils;
using Microsoft.EntityFrameworkCore;

namespace PostyTest;

public class CommentTest : IClassFixture<TestDatabaseFixture> 
{
          private readonly PostyContext context;
          private readonly ProfileRepository profileRepository;
          private readonly AuthService authService;
          private readonly SessionValidator sessionValidator;
          private readonly PostRepository postRepository;
          private readonly PostService postService;
          private ProfileSessionDto profile;
          private readonly CommentService commentService;
          private readonly CommentRepository commentRepository;

          public CommentTest()
          {
              var optionsBuilder = new DbContextOptionsBuilder<PostyContext>()
                   .UseInMemoryDatabase("CommentTest");
              context = new PostyContext(optionsBuilder.Options);
               
              profileRepository = new ProfileRepository(context);
              sessionValidator = new SessionValidator(profileRepository);
              authService = new AuthService(profileRepository, sessionValidator);
              postRepository = new PostRepository(context);
              postService = new PostService(postRepository, profileRepository, sessionValidator);
              commentRepository = new CommentRepository(context);
              commentService = new CommentService(commentRepository, postRepository, profileRepository, sessionValidator);
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
        public async void CommentPost_ShouldSucceed()
        {
   
            await Register();
            
            var newPost = new CreatePostDto
            {
                Text = "hello",
                Title = "first time"
            };

            var newComment = new CreateCommentDto
            {
                Text = "pirate"
            };
            
            var postResult = await postService.Add(newPost, profile.SessionId);
            var commentResult = await commentService.Add(postResult.Data.Id, newComment, profile.SessionId);
            var findPost = await commentService.GetById(commentResult.Data.Id);
            Assert.Equal("pirate", findPost.Data.Text);
        }        
           
        [Fact]
        public async void RemoveCommentPost_ShouldSucceed()
        {
            await Register();
            
            var newPost = new CreatePostDto
            {
                Text = "hello",
                Title = "first time"
            };

            var newComment = new CreateCommentDto
            {
                Text = "pirate"
            };
            
            var postResult = await postService.Add(newPost, profile.SessionId);
            var commentResult = await commentService.Add(postResult.Data.Id, newComment, profile.SessionId);
            var findPost = await commentService.GetById(commentResult.Data.Id);
            Assert.Equal("pirate", findPost.Data.Text);
            
            var removeCommentResult = await commentService.Delete(commentResult.Data.Id, profile.SessionId);
            Assert.True(removeCommentResult.Success);
            findPost = await commentService.GetById(commentResult.Data.Id);
            Assert.False(findPost.Success);
        }       
            
        [Fact]
        public async void LikeComment_ShouldSucceed()
        {
            await Register();
            
            var newPost = new CreatePostDto
            {
                Text = "hello",
                Title = "first time"
            };

            var newComment = new CreateCommentDto
            {
                Text = "pirate"
            };
            
            var postResult = await postService.Add(newPost, profile.SessionId);
            var commentResult = await commentService.Add(postResult.Data.Id, newComment, profile.SessionId);
            var findComment = await commentService.GetById(commentResult.Data.Id);
            Assert.Equal("pirate", findComment.Data.Text);
            
            var removeCommentResult = await commentService.Like(commentResult.Data.Id, profile.SessionId);
            findComment = await commentService.GetById(commentResult.Data.Id);
            Assert.Equal(1,findComment.Data.Likes);
        }             
             
        [Fact]
        public async void DislikeComment_ShouldSucceed()
        {
            await Register();
            
            var newPost = new CreatePostDto
            {
                Text = "hello",
                Title = "first time"
            };

            var newComment = new CreateCommentDto
            {
                Text = "pirate"
            };
            
            var postResult = await postService.Add(newPost, profile.SessionId);
            var commentResult = await commentService.Add(postResult.Data.Id, newComment, profile.SessionId);
            var findComment = await commentService.GetById(commentResult.Data.Id);
            Assert.Equal("pirate", findComment.Data.Text);
            
            var likeCommentResult = await commentService.Like(commentResult.Data.Id, profile.SessionId);
            Assert.True(likeCommentResult.Success);
            findComment = await commentService.GetById(commentResult.Data.Id);
            Assert.Equal(1,findComment.Data.Likes);
            
            var dislikeCommentResult = await commentService.Dislike(commentResult.Data.Id, profile.SessionId);
            Assert.True(dislikeCommentResult.Success);
            findComment = await commentService.GetById(commentResult.Data.Id);
            Assert.Equal(0,findComment.Data.Likes);           
        }       
}