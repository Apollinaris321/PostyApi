using System.Net.Mime;

namespace LearnApi.Models;

public class PostDto
{
     public long Id { get; set; }
     public string Title { get; set; }
     public string Text { get; set; }
     public string AuthorName { get; set; }
     public DateTime CreatedAt { get; set; }
     public int Likes { get; set; }
     
     public PostDto(){}

     public PostDto(Post post)
     {
          Id = post.Id;
          Title = post.Title;
          Text = post.Text;
          AuthorName = post.Profile.UserName ?? "";
          CreatedAt = post.CreatedAt;
          Likes = post.Likes;
     }
}