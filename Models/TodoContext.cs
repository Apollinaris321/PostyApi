using Microsoft.EntityFrameworkCore;

namespace LearnApi.Models;
public class TodoContext : DbContext
{
    public TodoContext(DbContextOptions<TodoContext> options)
        : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems { get; set; } = null!;
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<Worksheet> Worksheets { get; set; } = null!;
}