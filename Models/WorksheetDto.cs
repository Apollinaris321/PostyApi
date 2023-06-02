namespace LearnApi.Models;

public class WorksheetDto
{
     public long Id { get; set; }
     public string Title { get; set; }
     public string? Exercises { get; set; }
     public ProfileDto? Profile { get; set; }

     public WorksheetDto(Worksheet w)
     {
          Id = w.Id;
          Exercises = w.Exercises;
          Profile = w.Profile is not null ? new ProfileDto(w.Profile) : null;
          Title = w.Title;
     }
}