using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis.Elfie.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnApi.Models;
public class TodoContext : DbContext
{
    public TodoContext(DbContextOptions<TodoContext> options)
        : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        new WorksheetEntityConfiguration().Configure(modelBuilder.Entity<Worksheet>());

        modelBuilder.Entity<PostLike>()
            .HasKey(pl => new { pl.ProfileId, pl.PostId });
         
        modelBuilder.Entity<Profile>()
            .HasData(
                new Profile
                {
                    Id = 1,
                    Username = "John Doe",
                    Email = "aa@mail.com",
                    Worksheets = new List<Worksheet>(),
                    Posts = new List<Post>(),
                    Password = "hallo"                   
                }
            );

        modelBuilder.Entity<Post>()
            .HasData(
                new Post
                {
                    Id = 1,
                    ProfileId = 1,
                    Text = "first post",
                    Likes = 0,
                    Comments = new List<Comment>()
                }
            );

        modelBuilder.Entity<Comment>()
            .HasData(
                new Comment
                {
                    Id = 1,
                    ProfileId = 1,
                    PostId = 1,
                    Likes = 1,
                    Text = "first comment"
                }
            );

        modelBuilder.Entity<CommentLike>()
            .HasData(
                new CommentLike
                {
                    Id = 1,
                    ProfileId = 1,
                    CommentId = 1
                }
            );
        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<CommentLike> CommentLikes { get; set; } = null!;
    public DbSet<PostLike> Likes { get; set; } = null!;
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<Worksheet> Worksheets { get; set; } = null!;
}