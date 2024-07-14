namespace UserService.Models.ViewModels
{
    public class AddRolesToUserVM
    {
        public string UserId { get; set; }
        public List<string> RoleNames { get; set; }
    }
}