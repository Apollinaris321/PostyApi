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
                },
                new Profile
                {
                    Id = 3,
                    Username = "string",
                    Email = "string",
                    Worksheets = new List<Worksheet>(),
                    Password = BCrypt.Net.BCrypt.HashPassword("string")
                }
            );
        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<Worksheet> Worksheets { get; set; } = null!;
}