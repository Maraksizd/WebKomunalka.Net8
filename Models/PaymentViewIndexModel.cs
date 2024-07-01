using System.Collections.Generic;
using System.Linq;

namespace WebKomunalka.Net8.Models
{
    public class PaymentViewIndexModel
    {
        public List<Payment> Payments { get; set; }
        public List<Service> Services { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public PaymentViewIndexModel(List<Payment> payments, List<Service> services, int currentPage, int totalPages)
        {
            Payments = payments;
            Services = services;
            CurrentPage = currentPage;
            TotalPages = totalPages;
        }

        public string GetServiceNameById(int serviceId)
        {
            var service = Services.FirstOrDefault(s => s.Id == serviceId);
            return service != null ? service.ServiceName : "Unknown Service";
        }
    }
}