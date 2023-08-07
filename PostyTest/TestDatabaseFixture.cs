using LearnApi.Models;
using Microsoft.EntityFrameworkCore;

namespace PostyTest;

public class TestDatabaseFixture : IDisposable
{
    private readonly DbContextOptions<PostyContext> _options;
    public TestDatabaseFixture()
    {
        _options = new DbContextOptionsBuilder<PostyContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
    }

    public PostyContext CreateContext()
    {
        return new PostyContext(_options);
    }

    public void Dispose()
    {
        using (var context = CreateContext())
        {
            context.Database.EnsureDeleted();
        }
    }
}