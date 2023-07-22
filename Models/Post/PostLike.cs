using System.ComponentModel.DataAnnotations.Schema;

namespace LearnApi.Models;

public class PostLike
{
    public long PostId { get; set; }
    public Post Post { get; set; }
    public string ProfileId { get; set; }
    public Profile Profile { get; set; }
    public DateTime CreatedAt { get; set; }
}