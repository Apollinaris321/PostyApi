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

     public CommentDto()
     {
          CreatedAt = DateTime.Now;
          Likes = 0;
     }

     public CommentDto(Comment comment)
     {
          Id = comment.Id;
          Text = comment.Text;
          CreatedAt = comment.CreatedAt;
          AuthorName = comment.Profile.UserName ?? "EMPTY";
          PostId = comment.PostId;
          Likes = comment.LikedBy.Count;
          LikedByYou = false;
     }
     
     public CommentDto(Comment comment, string? profileId)
     {
          Id = comment.Id;
          Text = comment.Text;
          CreatedAt = comment.CreatedAt;
          AuthorName = comment.Profile.UserName ?? "EMPTY";
          PostId = comment.PostId;
          Likes = comment.LikedBy.Count;
          if (comment.LikedBy.FirstOrDefault(like => like.ProfileId == profileId) != null)
          {
               LikedByYou = true;
          }
     }
}