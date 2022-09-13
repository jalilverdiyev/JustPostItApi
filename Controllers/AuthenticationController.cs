using JustPostItAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace JustPostItAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{

    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(ILogger<AuthenticationController> logger)
    {
        _logger = logger;
    }

    [HttpPost("Login")]
    public Person Login(User user)
    {
        return DbController.Authenticate(user);
    }
    
    [HttpPost("Register")]
    public bool Register()
    {
        User user = new User();
        var postedData = HttpContext.Request.Form;
        var username = postedData["Username"];
        var email = postedData["Email"];
        var password = postedData["Password"];
        var photo = postedData.Files["ProfilePhoto"];
        user.UserName = username;
        user.Email = email;
        user.Password = password;
        user.ProfilePhoto = photo;
        return DbController.Add(user);
    }
   
    
}