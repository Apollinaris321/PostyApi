namespace LearnApi.Models;

public class ProfileDto
{
     public long Id { get; set; }
     public string Username { get; set; }
     public ICollection<Worksheet?> Worksheet { get; set; }

     public ProfileDto(Profile p)
     {
          Id = p.Id;
          Username = p.Username;
          Worksheet = p.Worksheet;
     }
}