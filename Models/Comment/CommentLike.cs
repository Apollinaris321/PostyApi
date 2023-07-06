using System.ComponentModel.DataAnnotations.Schema;

namespace LearnApi.Models;

public class CommentLike
{
    [ForeignKey("Profile")]
    public string ProfileId { get; set; }
    public Profile Profile { get; set; }
    public long CommentId { get; set; }
    public Comment Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}