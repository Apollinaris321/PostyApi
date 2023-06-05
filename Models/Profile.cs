namespace LearnApi.Models;

public class Profile
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public ICollection<Worksheet> Worksheets { get; set; }

    public Profile()
    {
        Worksheets = new List<Worksheet>();
    }
     public Profile(ProfileRegisterDto pR)
     {
         Username = pR.Username;
         Email = pR.Email;
         Password = pR.Email;
         Worksheets = new List<Worksheet>();
     }   
}