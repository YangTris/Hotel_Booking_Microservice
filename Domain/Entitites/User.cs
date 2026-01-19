using Microsoft.AspNetCore.Identity;

namespace Domain.Entitites
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}