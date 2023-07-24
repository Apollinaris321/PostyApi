using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.CodeAnalysis.Elfie.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LearnApi.Models;
public class PostyContext : IdentityDbContext<IdentityUser>
{
    public PostyContext(DbContextOptions<PostyContext> options)
        : base(options)
    {
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var hasher = new PasswordHasher<Profile>();
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<CommentLike>()
            .HasKey(cl => new { cl.ProfileId, cl.CommentId });
         
        modelBuilder.Entity<PostLike>()
            .HasKey(pl => new { pl.ProfileId, pl.PostId });

        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<CommentLike> CommentLikes { get; set; } = null!;
    public DbSet<PostLike> PostLikes { get; set; } = null!;
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
}