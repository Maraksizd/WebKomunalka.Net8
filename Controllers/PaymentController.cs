using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebKomunalka.Net8.Data;
using WebKomunalka.Net8.Models;

namespace WebKomunalka.Net8.Controllers;

public class PaymentController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    : Controller
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<IdentityUser> _userManager = userManager;

  public IActionResult Index(string? sortedBy)
{
    var cUser = _userManager.GetUserAsync(User).Result;

    if (cUser == null)
    {
        return View("NotRegistered");
    }

    var currentUserId = cUser.Id;
    List<Payment> payments = new List<Payment>();

    if (sortedBy != null)
    {
        switch (sortedBy)
        {
            case "Id":
                payments = _context.Payments
                    .Include(p => p.Service)
                    .Where(p => p.Service != null && p.Service.UserId == currentUserId)
                    .OrderByDescending(p => p.Id)
                    .ToList();
                break;
            case "Date":
                payments = _context.Payments
                    .Include(p => p.Service)
                    .Where(p => p.Service != null && p.Service.UserId == currentUserId)
                    .OrderByDescending(p => p.Date)
                    .ToList();
                break;
            case "AmountUsage":
                payments = _context.Payments
                    .Include(p => p.Service)
                    .Where(p => p.Service != null && p.Service.UserId == currentUserId)
                    .OrderByDescending(p => p.AmountUsage)
                    .ToList();
                break;
            case "TotalPrice":
                payments = _context.Payments
                    .Include(p => p.Service)
                    .Where(p => p.Service != null && p.Service.UserId == currentUserId)
                    .OrderByDescending(p => p.TotalPrice)
                    .ToList();
                break;
            case "ServiceName":
                payments = _context.Payments
                    .Include(p => p.Service)
                    .Where(p => p.Service != null && p.Service.UserId == currentUserId)
                    .OrderByDescending(p => p.Service.ServiceName)
                    .ToList();
                break;
        }
    }

    if (!payments.Any())
    {
        payments = _context.Payments.Include(p => p.Service)
            .Where(p => p.Service != null && p.Service.UserId == currentUserId).ToListAsync().Result;
    }

    return View(payments);
}

    public IActionResult AddPayment()
    {
        var currentUser = _userManager.GetUserAsync(User).Result;
        var currentUserId = currentUser.Id;

        var services = _context.Services.Where(s => s.UserId == currentUserId).ToList();

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

        // Створюємо об'єкт PaymentViewDetailModel та заповнюємо його даними
        var model = new PaymentViewDetailModel
        {
            Service = payment.Service,
            Payment = payment
        };

        model.Service.UserId = "";
        model.Service.Id = 0;

        model.Payment.Id = 0;
        model.Payment.ServiceId = 0;

        // Передаємо модель в вюху
        return View(model);
    }
}