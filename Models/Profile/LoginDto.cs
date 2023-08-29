using System.ComponentModel.DataAnnotations;

namespace LearnApi.Models;

public class LoginDto
{
    [Required]
    [MinLength(3)]
    public string Username { get; set; }
    [Required]
    [MinLength(3)]
    public string Password { get; set; }

    public LoginDto(string username, string password)
    {
        Username = username;
        Password = password;
    }
}