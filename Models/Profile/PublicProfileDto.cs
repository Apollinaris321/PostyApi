namespace LearnApi.Models;

public class PublicProfileDto
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<Post> Posts { get; set; }
    public ICollection<Comment> Comments { get; set; }
    public ICollection<CommentLike> LikedComments { get; set; }
    public ICollection<PostLike> LikedPosts { get; set; }
}