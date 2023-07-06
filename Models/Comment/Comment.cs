using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LearnApi.Models;

public class Comment
{
    public long Id { get; set; }
    public string Text { get; set; } 
    public int Likes { get; set; }
    public DateTime CreatedAt { get; set; }   
    [ForeignKey("Profile")]
    public string ProfileId { get; set; }
    public Profile Profile { get; set; }
    public long PostId { get; set; }
    [JsonIgnore] 
    public Post Post { get; set; }
    
    public Comment(){}
}