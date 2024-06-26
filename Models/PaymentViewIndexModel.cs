using System.Collections.Generic;
using System.Linq;

namespace WebKomunalka.Net8.Models
{
    public class PaymentViewIndexModel
    {
        public List<Payment> _uPayments { get; set; }
        public List<Service> _uServices { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public PaymentViewIndexModel(List<Payment> inputPayments, List<Service> inputServices, int currentPage, int totalPages)
        {
            _uPayments = inputPayments;
            _uServices = inputServices;
            CurrentPage = currentPage;
            TotalPages = totalPages;
        }

        public string GetServiceNameById(int serviceId)
        {
            var service = _uServices.FirstOrDefault(s => s.Id == serviceId);
            return service != null ? service.ServiceName : "Unknown Service";
        }
    }
}