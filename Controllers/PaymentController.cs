using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebKomunalka.Net8.Data;
using WebKomunalka.Net8.Models;

namespace WebKomunalka.Net8.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PaymentController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public string TakeUserId()
        {
            var cUser = _userManager.GetUserAsync(User).Result;
            return cUser?.Id ?? "NotFound";
        }

// Перевіряє чи поля для фільтрів/сортування не пусті
        public List<string> InputValidation(string? filterServiceName, string? filterAmountUsageMin,
            string? filterAmountUsageMax, string? filterTotalPriceMin, string? filterTotalPriceMax)
        {
            var notEmptyValues = new List<string>();

            if (!string.IsNullOrEmpty(filterServiceName))
            {
                notEmptyValues.Add(filterServiceName);
            }

            if (!string.IsNullOrEmpty(filterAmountUsageMin))
            {
                notEmptyValues.Add(filterAmountUsageMin);
            }

            if (!string.IsNullOrEmpty(filterAmountUsageMax))
            {
                notEmptyValues.Add(filterAmountUsageMax);
            }

            if (!string.IsNullOrEmpty(filterTotalPriceMin))
            {
                notEmptyValues.Add(filterTotalPriceMin);
            }

            if (!string.IsNullOrEmpty(filterTotalPriceMax))
            {
                notEmptyValues.Add(filterTotalPriceMax);
            }

            return notEmptyValues;
        }

        public List<Payment> PaymentSearch(string? filterServiceName, string? filterAmountUsageMin,
            string? filterAmountUsageMax, string? filterTotalPriceMin, string? filterTotalPriceMax)
        {
            var userId = TakeUserId();

            // Отримання всіх платежів користувача
            var userPayments = _context.Payments
                .Include(p => p.Service)
                .Where(p => p.Service != null && p.Service.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filterServiceName))
            {
                userPayments = userPayments.Where(p => p.Service.ServiceName.Contains(filterServiceName));
            }

            if (!string.IsNullOrEmpty(filterAmountUsageMin) &&
                double.TryParse(filterAmountUsageMin, out double minUsage))
            {
                userPayments = userPayments.Where(p => p.AmountUsage >= minUsage);
            }

            if (!string.IsNullOrEmpty(filterAmountUsageMax) &&
                double.TryParse(filterAmountUsageMax, out double maxUsage))
            {
                userPayments = userPayments.Where(p => p.AmountUsage <= maxUsage);
            }

            if (!string.IsNullOrEmpty(filterTotalPriceMin) && double.TryParse(filterTotalPriceMin, out double minPrice))
            {
                userPayments = userPayments.Where(p => p.TotalPrice >= minPrice);
            }

            if (!string.IsNullOrEmpty(filterTotalPriceMax) && double.TryParse(filterTotalPriceMax, out double maxPrice))
            {
                userPayments = userPayments.Where(p => p.TotalPrice <= maxPrice);
            }

            return userPayments.ToList();
        }

// Повертає відсортовані оплати
        public IQueryable<Payment> GetSortedPayments(string sortedBy, IQueryable<Payment> payments)
        {
            return sortedBy switch
            {
                "Id" => payments.OrderByDescending(payment => payment.Id),
                "AmountUsage" => payments.OrderBy(payment => payment.AmountUsage),
                "ServiceId" => payments.OrderBy(payment => payment.ServiceId),
                "Date" => payments.OrderBy(payment => payment.Date),
                "TotalPrice" => payments.OrderBy(payment => payment.TotalPrice),
                _ => payments.OrderByDescending(payment => payment.Id)
            };
        }

        public List<Service> GetUserServices(string userId)
        {
            return _context.Services.Where(s => s.UserId == userId).ToList();
        }

        public IActionResult Index(string? sortedBy, string? filterServiceName, string? filterAmountUsageMin,
            string? filterAmountUsageMax, string? filterTotalPriceMin, string? filterTotalPriceMax, int page = 1)
        {
            var currentUserId = TakeUserId();

            var searchParameters = InputValidation(filterServiceName, filterAmountUsageMin, filterAmountUsageMax,
                filterTotalPriceMin, filterTotalPriceMax);
            var userServices = GetUserServices(currentUserId);

            int pageSize = 8;
            var userPayments = _context.Payments
                .Include(p => p.Service)
                .Where(p => p.Service != null && p.Service.UserId == currentUserId)
                .AsQueryable();

            if (searchParameters.Count > 0)
            {
                userPayments = PaymentSearch(filterServiceName, filterAmountUsageMin, filterAmountUsageMax,
                    filterTotalPriceMin, filterTotalPriceMax).AsQueryable();
            }

            userPayments = GetSortedPayments(sortedBy, userPayments);

            int totalPayments = userPayments.Count();
            int totalPages = (int)Math.Ceiling(totalPayments / (double)pageSize);

            var pagedPayments = userPayments
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new PaymentViewIndexModel(pagedPayments, userServices, page, totalPages);

            return View(model);
        }


        public IActionResult AddPayment()
        {
            var userId = TakeUserId();

            var services = _context.Services.Where(s => s.UserId == userId).ToList();

            ViewBag.UserServices = services;

            Payment newPayment = new Payment();

            return View(newPayment);
        }


        [HttpPost]
        public IActionResult AddPaymentPost(Payment newPayment)
        {
            if (ModelState.IsValid)
            {
                newPayment.Service = null;

                _context.Payments.Add(newPayment);

                _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            else
            {
                return View("Index");
            }
        }

        public IActionResult DeletePayment(int id)
        {
            var payment = _context.Payments.Find(id);

            if (payment == null)
            {
                return NotFound();
            }

            _context.Payments.Remove(payment);
            _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public IActionResult DetailsPayment(int id)
        {
            var payment = _context.Payments.Include(p => p.Service).FirstOrDefault(p => p.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            var model = new PaymentViewDetailModel
            {
                Service = payment.Service,
                Payment = payment
            };

            model.Service.UserId = "";
            model.Service.Id = 0;

            model.Payment.Id = 0;
            model.Payment.ServiceId = 0;

            return View(model);
        }
    }
}