
namespace WebKomunalka.Net8.Models
{
    public class ServiceViewIndexModel
    {
        public List<Service> Services { get; set; } = new List<Service>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}