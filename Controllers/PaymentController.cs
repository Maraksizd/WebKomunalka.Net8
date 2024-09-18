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

        public List<Service> GetUserServices(string userId)
        {
            return _context.Services.Where(s => s.UserId == userId).ToList();
        }

        public bool IsUserLogined()
        {
            return _userManager.GetUserAsync(User).Result != null;
        }

        // Перевіряє чи поля для фільтрів/сортування не пусті
        public List<string> InputValidation(params string?[] filters)
        {
            return filters.Where(filter => !string.IsNullOrEmpty(filter)).ToList();
        } // ready

        public List<Payment> TakeSoprtedPaymenets()
        {
            var currentUserId = TakeUserId();
            var usersServices = GetUserServices(currentUserId);

            var payments = _context.Payments.Include(p => p.Service)
                .Where(p => p.Service != null && p.Service.UserId == currentUserId)
                .ToList();

            return payments;
        }


        public IActionResult Index(string? sortedBy, string? filterServiceName, string? filterAmountUsageMin,
            string? filterAmountUsageMax, string? filterTotalPriceMin, string? filterTotalPriceMax, int page = 1)
        {
            // add check user logined or not

            var loginStatus = IsUserLogined();

            if (!loginStatus)
            {
                return RedirectToAction("Index", "Home");
            }

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
            }


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