using System.Net;
using System.Text;
using LearnApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;

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
     
    [Fact]
    public async Task Login_Success()
    {
        using var client = _factory.CreateClient();

        var loginData = new LoginDto("joe", "Hallo123,");
        var loginJson = JsonSerializer.Serialize(loginData);
        var response = await client
            .PostAsync("api/Profile/register", new StringContent(loginJson, Encoding.UTF8, "application/json"));

        var str = await response.Content.ReadAsStringAsync();
        var respOb = JsonSerializer.Deserialize<ProfileDto>(await response.Content.ReadAsStreamAsync());
        Assert.Equal(loginData.Username , respOb.Username );
    }   
}