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
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public string TakeUserId()
        {
            var cUser = _userManager.GetUserAsync(User).Result;
            return cUser?.Id ?? "NotFound";
        }

        public List<Service> GetUserServices(string userId)
        {
            return _context.Services.Where(s => s.UserId == userId).ToList();
        }

        public bool IsUserLogined()
        {
            return _userManager.GetUserAsync(User).Result != null;
        }

        public PaymentViewIndexModel CompleteTheModel(List<Payment> userPayments, List<Service> userServices,
            int currentPage, int totalPages)
        {
            return new PaymentViewIndexModel(userPayments, userServices, currentPage, totalPages);
        }

        public List<string> InputValidation(params string?[] filters)
        {
            return filters.Where(filter => !string.IsNullOrEmpty(filter)).ToList();
        }

        public List<Payment> TakePaymenets()
        {
            var currentUserId = TakeUserId();
            var usersServices = GetUserServices(currentUserId);

            var payments = _context.Payments
                .Include(p => p.Service)
                .Where(p => usersServices.Select(s => s.Id).Contains(p.ServiceId))
                .ToList();

            return payments;
        }

        public List<Payment> SortPayments(List<Payment> payments, string sortedBy)
        {
            switch (sortedBy)
            {
                case "Date":
                    return payments.OrderByDescending(p => p.Date).ToList();
                case "AmountUsage":
                    return payments.OrderByDescending(p => p.AmountUsage).ToList();
                case "TotalPrice":
                    return payments.OrderByDescending(p => p.TotalPrice).ToList();
                default:
                    return payments.OrderByDescending(p => p.Id).ToList();
            }
        }

        public IActionResult Index(string? sortedBy, string? filterServiceName, string? filterAmountUsageMin,
            string? filterAmountUsageMax, string? filterTotalPriceMin, string? filterTotalPriceMax, int page = 1)
        {
            if (!IsUserLogined())
            {
                return RedirectToAction("Index", "Home");
            }

            var currentUserId = TakeUserId();
            var userPaymentsList = TakePaymenets();
            var userServices = GetUserServices(currentUserId);

            if (sortedBy == null)
            {
                sortedBy = "Id"; // За замовчуванням сортування за Id
            }

            // Сортуємо платежі за спаданням Id
            var sortedPayments = SortPayments(userPaymentsList, sortedBy);

            int pageSize = 8;
            var searchParams = InputValidation(filterServiceName, filterAmountUsageMin, filterAmountUsageMax,
                filterTotalPriceMin, filterTotalPriceMax);

            if (searchParams.Count == 0)
            {
                int totalPayments = sortedPayments.Count();
                int totalPages = (int)Math.Ceiling(totalPayments / (double)pageSize);

                // Підтримка пагінації
                var pagedPayments = sortedPayments
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var model = CompleteTheModel(pagedPayments, userServices, page, totalPages);
                return View(model);
            }

            // Додаємо фільтрацію тут, якщо потрібно

            int filteredPaymentsCount = sortedPayments.Count();
            int filteredTotalPages = (int)Math.Ceiling(filteredPaymentsCount / (double)pageSize);

            var filteredPagedPayments = sortedPayments
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var filteredModel = CompleteTheModel(filteredPagedPayments, userServices, page, filteredTotalPages);
            return View(filteredModel);
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
        public async Task<IActionResult> AddPaymentPost(Payment newPayment)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                newPayment.UserId = currentUser.Id;
                newPayment.Service = null; // Ensure Service is not set to a new object

                _context.Payments.Add(newPayment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                return View("Index");
            }
        }

        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DetailsPayment(int id)
        {
            var payment = await _context.Payments.Include(p => p.Service).FirstOrDefaultAsync(p => p.Id == id);
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