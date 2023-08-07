using System.ComponentModel.DataAnnotations.Schema;

namespace LearnApi.Models;

public class CommentLike
{
    public string ProfileId { get; set; }
    public long CommentId { get; set; }
    public DateTime CreatedAt { get; set; }
}