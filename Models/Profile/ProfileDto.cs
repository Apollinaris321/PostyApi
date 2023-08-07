namespace LearnApi.Models;

public class ProfileDto
{
    public string Username { get; set;}
    public string Email { get; set;} 
    public ProfileDto(){}

    public ProfileDto(Profile p)
    {
        Username = p.Username ?? "empty";
        Email = p.Email ?? "empty";           
    }
}