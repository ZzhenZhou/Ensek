using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ensek.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
       
        // POST api/<ValuesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {

        }
    }
}
