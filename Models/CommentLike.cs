namespace LearnApi.Models;

public class CommentLike
{
    public long Id { get; set; }
    public long ProfileId { get; set; }
    public Profile Profile { get; set; }
    public long CommentId { get; set; }
    public Comment Comment { get; set; }
}