using System.Collections;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace LearnApi.Models;

public class Profile
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public Role Role { get; set; } = Role.User;
    public string SessionId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    [JsonIgnore] public ICollection<Post> Posts { get; set; } = new List<Post>();
    [JsonIgnore] public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    [JsonIgnore] public ICollection<PostLike> LikedPosts { get; set; } = new List<PostLike>();
    [JsonIgnore] public ICollection<CommentLike> LikedComments { get; set; } = new List<CommentLike>();

    public Profile(){}
}