using System.Net;
using LearnApi.Models;
using LearnApi.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Newtonsoft.Json;

namespace PostyTest;

public class ProfileControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ProfileControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task Register_ExpectFail()
    {
        using var client = _factory.CreateClient();
        
        var response = await client.GetAsync($"api/Profile/{"string"}");
        
        Assert.Equal(HttpStatusCode.BadRequest , response.StatusCode);
    }
    
    [Fact]
    public async Task Register_HappyPath()
    {
        using var client = _factory.CreateClient();
        
        var response = await client.GetAsync("api/Profile");
        
        Assert.Equal(HttpStatusCode.OK , response.StatusCode);
    }
}