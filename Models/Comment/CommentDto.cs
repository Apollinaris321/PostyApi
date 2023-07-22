namespace LearnApi.Models;

public class CommentDto
{
     public long Id { get; set; }
     public string Text { get; set; } 
     public int Likes { get; set; }
     public DateTime CreatedAt { get; set; }   
     public string AuthorName { get; set; }
     public long PostId { get; set; }   
     public bool LikedByYou { get; set; }
     
     public CommentDto(){}

     public CommentDto(Comment comment)
     {
          Id = comment.Id;
          Text = comment.Text;
          CreatedAt = comment.CreatedAt;
          AuthorName = comment.Profile.UserName ?? "EMPTY";
          PostId = comment.PostId;
          Likes = comment.Likes;
          LikedByYou = false;
     }
     
     public CommentDto(Comment comment, string usernameHasLiked)
     {
          Id = comment.Id;
          Text = comment.Text;
          CreatedAt = comment.CreatedAt;
          AuthorName = comment.Profile.UserName ?? "EMPTY";
          PostId = comment.PostId;
          Likes = comment.LikedBy.Count;
          if (comment.LikedBy.FirstOrDefault(like => like.ProfileId == usernameHasLiked) != null)
          {
               LikedByYou = true;
          }
     }
}