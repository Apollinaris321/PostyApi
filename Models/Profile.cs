using System.Collections;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LearnApi.Models;

public class Profile
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    [JsonIgnore]
    public string Password { get; set; }
    [JsonIgnore]
    public ICollection<Post> Posts { get; set; }
    [JsonIgnore]
    public ICollection<Comment> Comments { get; set; }
    [JsonIgnore]
    public ICollection<PostLike> LikedPosts { get; set; }

    [JsonIgnore]
    public ICollection<Worksheet> Worksheets { get; set; }

    public Profile()
    {
        Worksheets = new List<Worksheet>();
    }
     public Profile(string _username, string _email, string _password)
     {
         Username = _username;
         Email = _email;
         Password = _password;
         Worksheets = new List<Worksheet>();
     }   
}