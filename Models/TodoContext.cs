using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LearnApi.Models;
public class TodoContext : IdentityUserContext<IdentityUser>
{
    public TodoContext(DbContextOptions<TodoContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // modelBuilder.Entity<Profile>()
        //     .Property(p => p.Username)
        //     .IsRequired();
        base.OnModelCreating(modelBuilder);
         
        modelBuilder.Entity<Profile>()
            .HasData(
                new Profile
                {
                    Id = 1,
                    Username = "John Doe",
                    Email = "aa@mail.com",
                    Worksheets = new List<Worksheet>(),
                    Password = "hallo"                   
                },
                new Profile
                {
                    Id = 2,
                    Username = "anni pa",
                    Email = "aa@mail.com",
                    Worksheets = new List<Worksheet>(),
                    Password = "hallo"
                }
            );
          
        modelBuilder.Entity<Worksheet>()
            .HasData(
                new Worksheet()
                {
                    Id = 1,
                    Exercises = "1+1=2",
                    Title = "my form 1",
                    ProfileId = null
                },
                new Worksheet()
                {
                    Id = 2,
                    Exercises = "1+1=2",
                    Title = "my form 1",
                    ProfileId = null
                }
            );              
        
        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<TodoItem> TodoItems { get; set; } = null!;
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<Worksheet> Worksheets { get; set; } = null!;
}