using System.Net.Mime;

namespace LearnApi.Models;

public class PostDto
{
     public long Id { get; set; }
     public string Text { get; set; }
     public string AuthorName { get; set; }
     public DateTime CreatedAt { get; set; }
     public int Likes { get; set; }
     public bool LikedByYou { get; set; }
     
     public PostDto(){}

     public PostDto(Post post, string profileId)
     {
          Id = post.Id;
          Text = post.Text;
          AuthorName = post.Profile.UserName ?? "";
          CreatedAt = post.CreatedAt;
          Likes = post.ProfileLikes.Count;
          if (post.ProfileLikes.FirstOrDefault(like => like.ProfileId == profileId) != null)
          {
               LikedByYou = true;
          }
     }
     
     public PostDto(Post post)
     {
          Id = post.Id;
          Text = post.Text;
          AuthorName = post.Profile.UserName ?? "";
          CreatedAt = post.CreatedAt;
          Likes = post.ProfileLikes.Count;
          LikedByYou = false;
     }
}