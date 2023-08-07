namespace LearnApi.Models;

public class ProfileSessionDto
{
     public string Username { get; set;}
     public string Email { get; set;}    
     public string SessionId { get; set; }

     public ProfileSessionDto(Profile profile)
     {
          Username = profile.Username;
          Email = profile.Email;
          SessionId = profile.SessionId;
     }
}