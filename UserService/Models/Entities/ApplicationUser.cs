using Microsoft.AspNetCore.Identity;

namespace UserService.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}