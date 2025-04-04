using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("ApiServer/[action]")]
    public class ApiServerController : ControllerBase
    {
        [HttpGet(Name = "Add")]
        public Task<int> Add(int a, int b)
        {
            return Task.FromResult(a + b);
        }
    }
}