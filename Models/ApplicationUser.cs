using Microsoft.AspNetCore.Identity;

namespace WebKomunalka.Net8.Models;

public class ApplicationUser : IdentityUser
{
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
}