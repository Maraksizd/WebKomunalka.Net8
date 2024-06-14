using System.Collections.Generic;
using System.Linq;

namespace WebKomunalka.Net8.Models
{
    public class PaymentViewIndexModel
    {
        public List<Payment> _uPayments { get; set; }
        public List<Service> _uServices { get; set; }

        public PaymentViewIndexModel(List<Payment> inputPayments, List<Service> inputServices)
        {
            _uPayments = inputPayments;
            _uServices = inputServices;
        }

        public string GetServiceNameById(int serviceId)
        {
            var service = _uServices.FirstOrDefault(s => s.Id == serviceId);
            return service != null ? service.ServiceName : "Unknown Service";
        }
    }
}