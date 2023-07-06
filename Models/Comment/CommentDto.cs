namespace LearnApi.Models;

public class CommentDto
{
     public long Id { get; set; }
     public string Text { get; set; } 
     public int Likes { get; set; }
     public DateTime CreatedAt { get; set; }   
     public string AuthorName { get; set; }
     public long PostId { get; set; }   
     
     public CommentDto(){}

     public CommentDto(Comment comment)
     {
          Id = comment.Id;
          Text = comment.Text;
          Likes = comment.Likes;
          CreatedAt = comment.CreatedAt;
          AuthorName = comment.Profile.UserName ?? "EMPTY";
          PostId = comment.PostId;
     }
}