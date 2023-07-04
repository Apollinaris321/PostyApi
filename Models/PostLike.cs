namespace LearnApi.Models;

public class PostLike
{
    public long PostId { get; set; }
    public Post Post { get; set; }
    public long ProfileId { get; set; }
    public Profile Profile { get; set; }
}