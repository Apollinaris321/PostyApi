using System.ComponentModel.DataAnnotations.Schema;

namespace LearnApi.Models;

public class CommentLike
{
    public long ProfileId { get; set; }
    public long CommentId { get; set; }
    public DateTime CreatedAt { get; set; }
}