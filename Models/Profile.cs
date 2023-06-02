namespace LearnApi.Models;

public class Profile
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public ICollection<Worksheet?> Worksheet { get; set; }
}