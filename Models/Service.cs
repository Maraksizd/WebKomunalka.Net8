using Microsoft.AspNetCore.Identity;

namespace WebKomunalka.Net8.Models;

public class Service
{
    public int Id { get; set; } = 0;

    public string ServiceName { get; set; } = String.Empty;

    public double UnitPrice { get; set; } = 0;

    public string UnitType { get; set; } = String.Empty;

    public string Company { get; set; } = String.Empty;

    public string UserId { get; set; } = String.Empty;

    public ApplicationUser? User { get; set; } = new ApplicationUser();
}