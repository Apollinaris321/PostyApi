using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LearnApi.Models;

public class Comment
{
    public long Id { get; set; }
    public string Text { get; set; } 
    public int Likes { get; set; }
    [JsonIgnore]
    public ICollection<CommentLike> LikedBy { get; set; }
    public DateTime CreatedAt { get; set; }   
    public string ProfileId { get; set; }
    [JsonIgnore]
    public Profile Profile { get; set; }
    public long PostId { get; set; }
    [JsonIgnore] 
    public Post Post { get; set; }

    public Comment()
    {
        Likes = 0;
        CreatedAt = DateTime.Now;
        LikedBy = new List<CommentLike>();
    }
}