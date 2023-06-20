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

    public Worksheet(string _title, string _exercises, long _profileId)
    {
        Title = _title;
        Exercises = _exercises;
        ProfileId = _profileId;
    }
}