using Microsoft.AspNetCore.Identity;

namespace Trips.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; }

         public string Email { get; set; }
    }
    public class ApplicationRole : IdentityRole
{
   
}
}



