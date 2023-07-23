using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LearnApi.Models;

public class Post
{
    public long Id { get; set; }
    [ForeignKey("Profile")]
    public string ProfileId { get; set; }
    public Profile Profile { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Likes { get; set; }
    [JsonIgnore]
    public ICollection<Comment> Comments { get; set; }
    [JsonIgnore]
    public ICollection<PostLike> ProfileLikes { get; set; }

    public Post()
    {
        ProfileLikes = new List<PostLike>();
        Likes = 0;
        CreatedAt = DateTime.Now;
    }
}
