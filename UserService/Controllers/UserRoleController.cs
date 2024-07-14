using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary;
using UserService.Models.Entities;
using UserService.Models.ViewModels;

namespace UserService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserRoleController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRoleController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("GetRolesForUser/{userId}")]
        [Authorize(Policy = nameof(SystemPermissions.GetRoleForUser))]
        public async Task<IActionResult> GetRolesForUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                var roles = await _userManager.GetRolesAsync(user);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("AddRolesToUser")]
        [Authorize(Policy = nameof(SystemPermissions.AssignRoleToUser))]
        public async Task<IActionResult> AddRolesToUser([FromBody] AddRolesToUserVM model)
        {
            if (model == null || string.IsNullOrEmpty(model.UserId))
            {
                return BadRequest("Invalid model or empty role list");
            }
            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                foreach (var roleName in model.RoleNames)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        return BadRequest($"Roles is invalid!");
                    }
                }
                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return BadRequest(removeResult.Errors);
                }

                foreach (var roleName in model.RoleNames)
                {
                    var result = await _userManager.AddToRoleAsync(user, roleName);
                    if (!result.Succeeded)
                    {
                        return BadRequest(result.Errors);
                    }
                }
                return Ok("Roles added to user successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}