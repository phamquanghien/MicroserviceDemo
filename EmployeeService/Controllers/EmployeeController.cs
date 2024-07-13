using Microsoft.AspNetCore.Mvc;
namespace EmployeeService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<string> employees = new List<string>() { "Employee 1", "Employee 2" };
            return Ok(employees);
        }
    }
}