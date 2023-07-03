using System.Collections;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LearnApi.Models;

public class Post
{
    public long Id { get; set; }
    public long ProfileId { get; set; }
    public Profile Profile { get; set; }
    public string Text { get; set; }
    public int Likes { get; set; }
    [JsonIgnore]
    public ICollection<Comment> Comments { get; set; }
    [JsonIgnore]
    public ICollection<PostLike> ProfileLikes { get; set; }
    
    public Post(){}

    public Post(string text, Profile profile)
    {
        Likes = 0;
        Text = text;
        ProfileId = profile.Id;
        Profile = profile;
    }
    
    public Post(long profileId, string text)
    {
        Likes = 0;
        ProfileId = profileId;
        Text = text;
    }
}
