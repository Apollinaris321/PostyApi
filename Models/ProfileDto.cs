namespace LearnApi.Models;

public class ProfileDto
{
    public string Username { get; set;}
    public string Email { get; set;} 
    public string Password { get; set;}

    public ProfileDto(){}
    public ProfileDto(string _username, string _email, string _password)
    {
        Username = _username;
        Email = _email;
        Password = _password;
    }
}