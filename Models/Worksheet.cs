namespace LearnApi.Models;

public class Worksheet
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string? Exercises { get; set; }
    public long? ProfileId { get; set; }

    public Worksheet()
    {
    }
    
    public Worksheet(WorksheetDto worksheetDto)
    {
        Title = worksheetDto.Title;
        Exercises = worksheetDto.Exercises;
    }
}