namespace LearnApi.Models;

public class WorksheetDto
{
    public string Title { get; set; }
    public List<List<Symbol>> Exercises { get; set; }
    public long? ProfileId { get; set; }

    public WorksheetDto()
    {
    }
}