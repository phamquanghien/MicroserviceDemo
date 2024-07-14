using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace UserService.Models.Entities
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime Created { get; set; }
        public DateTime? Revoked { get; set; }
        public bool IsRevoked => Revoked != null;
        public bool IsActive => !IsExpired && !IsRevoked;
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}