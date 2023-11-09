using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BaseProject.API.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class ServiceController : Controller
    {
        public ServiceController() { }
        
        [AllowAnonymous]
        [HttpPost("FileCallback")]
        public IActionResult FileCallback()
        {
            return Ok();
        }

    }
}