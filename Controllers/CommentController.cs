using LearnApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace LearnApi.Controllers;

public class CommentController : ControllerBase
{
    private readonly TodoContext _context;
    private readonly IConfiguration _configuration; 
    public CommentController(TodoContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }


}