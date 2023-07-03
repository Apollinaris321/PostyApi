using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace LearnApi.Models;

public class RegisterDto
{
     [Required]
     [MinLength(3)]
     public string Username { get; set; }
     [Required]
     [EmailAddress]
     public string Email { get; set; }
     [Required]
     [MinLength(3)]
     public string Password { get; set; }   
     
     public RegisterDto(){}
}