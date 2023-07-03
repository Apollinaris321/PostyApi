using System.Text.Json;
using System.Text.Json.Serialization;

namespace LearnApi.Models;

public class Comment
{
    public long Id { get; set; }
    public string Text { get; set; } 
    public long ProfileId { get; set; }
    public Profile Profile { get; set; }
    public int Likes { get; set; }
    public long PostId { get; set; }
    [JsonIgnore] 
    public Post Post { get; set; }
    
    public Comment(){}

    public Comment(long postId, long profileId, string text)
    {
        Text = text;
        ProfileId = profileId;
        PostId = postId;
    }
}