using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;

namespace LearnApi.Models;

public class PostLike
{
    public long PostId { get; set; }
    public long ProfileId { get; set; }
    public DateTime CreatedAt { get; set; } 
}