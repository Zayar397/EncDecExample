using EncDecExample.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EncDecExample.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BlogController : ControllerBase
{
    private readonly EncDecService _encDecService;

    public BlogController(EncDecService encDecService)
    {
        _encDecService = encDecService;
    }

    [HttpPost("Login")]
    public IActionResult Login(BlogLoginRequestModel requestModel)
    {
        try
        {
            var result = UserData.user.FirstOrDefault(x => x.Username == requestModel.Username &&
                                                            x.Password == requestModel.Password);
            if (result is null)
            {
                return Unauthorized();
            }

            var user = new BlogLoginModel
            {
                Username = result.Username,
                SessionId = Guid.NewGuid().ToString(),
                SessionExpired = DateTime.Now.AddMinutes(1)
            };
            var jsonStr = JsonConvert.SerializeObject(user);
            var encryptedStr = _encDecService.Encrypt(jsonStr);
            var model = new BlogLoginResponseModel
            {
                AccessToken = encryptedStr
            };

            return Ok(model);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }
    [HttpPost("UserList")]
    public IActionResult UserList(UserListRequestModel requestModel)
    {
        try
        {
            var json = _encDecService.Decrypt(requestModel.AccessToken);
            var user = JsonConvert.DeserializeObject<BlogLoginModel>(json);
            if (user.SessionExpired < DateTime.Now)
            {
                return Unauthorized("Session is expired.");
            }
            return Ok(UserData.user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }
}
public class BlogLoginRequestModel
{
    public string Username { get; set; }
    public string Password { get; set; }
}
public class BlogLoginResponseModel
{
    public string AccessToken { get; set; }
}
public class UserListRequestModel
{
    public string AccessToken { get; set; }
}
public class BlogLoginModel
{
    public string Username { get; set; }
    public string SessionId { get; set; }
    public DateTime SessionExpired { get; set; }
}
public static class UserData
{
    public static List<UserDto> user = new List<UserDto>
    {
        new UserDto{Username = "admin", Password = "p@ssw0rd" },
        new UserDto{Username = "user", Password = "p@ssw0rd" },
        new UserDto{Username = "guest", Password = "p@ssw0rd" }
    };
}
public class UserDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}
