using FileAccessApp.Handlers;
using FileAccessApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;


namespace FileAccessApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromServices] LoginHandler handler, [FromBody] Credentials credentials)
        {
            var token = handler.Handle(credentials);

            if (token == null)
            {
                return Unauthorized("Username and password does not match");
            }

            return Ok(new { token });
        }

        [Authorize]
        [HttpGet("secure")]
        public IActionResult SecureEndpoint()
        {
            return Ok("Valid token");
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok("Test succesful");
        }
    }
}