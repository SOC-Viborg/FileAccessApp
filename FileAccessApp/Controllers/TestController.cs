using Microsoft.AspNetCore.Mvc;

namespace FileAccessApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController: ControllerBase
    {

        [HttpGet("test")]
        public ActionResult Test()
        {
            return Ok("Hello Wolrd");
        }
    }
}
