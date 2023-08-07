using System.ComponentModel.DataAnnotations.Schema;

namespace LearnApi.Models;

public class PostLike
{
    public long PostId { get; set; }
    public long ProfileId { get; set; }
}