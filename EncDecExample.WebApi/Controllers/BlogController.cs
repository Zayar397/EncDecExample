using System.Globalization;
using EncDecExample.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
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
                SessionExpired = DateTime.Now.AddMinutes(15)
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
            bool hasValue = HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
            if (hasValue == false)
            {
                return Unauthorized("Access token is required.");
            }
            var json = _encDecService.Decrypt(token.ToString());
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

    [ServiceFilter(typeof(ValidationTokenActionFilter))]
    [HttpPost("UserListWithFilter")]
    public IActionResult UserListWithFilter(UserListRequestModel requestModel)
    {
        try
        {
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
    public string? AccessToken { get; set; }
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
public class ValidationTokenActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Do something before the action executes.
        var result = context.HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
        if (result == false)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var encDecService = context.HttpContext.RequestServices.GetRequiredService<EncDecService>();
        var json = encDecService.Decrypt(token.ToString());
        var user = JsonConvert.DeserializeObject<BlogLoginModel>(json);
        if (user.SessionExpired < DateTime.Now)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        await next();
        // Do something after the action executes.
    }
}
public class ValidationTokenMiddleware
{
    private readonly RequestDelegate _next;

    public ValidationTokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        //if (context.Request.Path.ToString().ToLower() == "/weatherforecast")
        //{
        //    goto Result;
        //}
        string requestPath = context.Request.Path.ToString().ToLower();
        if (allowList.Contains(requestPath))
        {
            goto Result;
        }
        bool hasVal = context.Request.Headers.TryGetValue("Authorization", out var accessToken);
        if (!hasVal)
        {
            context.Response.StatusCode = 401;
            return;
        }

        var encDec = context.RequestServices.GetRequiredService<EncDecService>();
        var jsonStr = encDec.Encrypt(accessToken.ToString());
        var resultObj = JsonConvert.DeserializeObject<BlogLoginModel>(jsonStr);
        if (resultObj.SessionExpired < DateTime.Now)
        {
            context.Response.StatusCode = 401;
            return;
        }
    // Call the next delegate/middleware in the pipeline.
    Result:
        await _next(context);
    }

    private string[] allowList =
    {
        "/weatherforecast",
        "/api/blog/login"
    };
}
public static class ValidationTokenMiddlewareExtensions
{
    public static IApplicationBuilder UseValidationTokenMiddleware(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<ValidationTokenMiddleware>();
    }
}
