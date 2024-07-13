namespace MemberUnitService.Controllers
{

    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class MemberUnitController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<string> members = new List<string>() { "Member Unit 1", "Member Unit 2" };
            return Ok(members);
        }
    }
}