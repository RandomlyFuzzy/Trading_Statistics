using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebRouter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RestController : Controller
    {
        private readonly ILogger<RestController> _logger;

        public RestController(ILogger<RestController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "RestController")]
        public IEnumerable<TempData> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new TempData
            {
                Details = "" + index
            })
            .ToArray();
        }




    }
}
