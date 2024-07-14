using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary;

namespace UserService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RolePermissionController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        public RolePermissionController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }
        [HttpPost("{roleId}/permissions")]
        [Authorize(Policy = nameof(SystemPermissions.AssignPermissionToRole))]
        public async Task<IActionResult> AddPermissionsToRole(string roleId, [FromBody] List<SystemPermissions> permissions)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound("Role not found");
            }
            //remove old claims
            var claims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in claims.Where(c => c.Type == "Permission"))
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }
            //add new claims
            var validPermissions = Enum.GetNames(typeof(SystemPermissions));
            foreach (var permission in permissions)
            {
                var permissionName = permission.ToString();
                if (!validPermissions.Contains(permissionName))
                {
                    return BadRequest($"Permission is not valid.");
                }
            }

            foreach (var permission in permissions)
            {
                var permissionName = permission.ToString();
                var claim = new Claim("Permission", permissionName);
                var result = await _roleManager.AddClaimAsync(role, claim);
                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }
            }

            return Ok("Permissions added to role");
        }
        [HttpGet("{roleId}/permissions")]
        [Authorize(Policy = nameof(SystemPermissions.GetPermissionForRole))]
        public async Task<IActionResult> GetPermissionsForRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound("Role not found.");
            }

            var claims = await _roleManager.GetClaimsAsync(role);
            var permissions = new List<string>();
            foreach (var claim in claims)
            {
                if (claim.Type == "Permission")
                {
                    permissions.Add(claim.Value);
                }
            }

            return Ok(permissions);
        }
    }
}