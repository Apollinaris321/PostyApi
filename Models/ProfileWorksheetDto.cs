namespace LearnApi.Models;

public class ProfileWorksheetDto
{
     public long Id { get; set; }
     public string Username { get; set; }

     public ICollection<Worksheet?> Worksheets { get; set; }
     
     public ProfileWorksheetDto(Profile p)
     {
          Id = p.Id;
          Username = p.Username;
          Worksheets = p.Worksheets;
     }
}