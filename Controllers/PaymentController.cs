using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebKomunalka.Net8.Data;
using WebKomunalka.Net8.Models;

namespace WebKomunalka.Net8.Controllers;

public class PaymentController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly ApplicationDbContext context;

    public PaymentController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        this.userManager = userManager;
        this.context = context;
    }

    public async Task<string> TakeUserIdAsync()
    {
        var cUser = await userManager.GetUserAsync(User);
        return cUser?.Id ?? string.Empty;
    }

    // Перевіряє чи поля для фільтрів/сортування не пусті
    public List<string> InputValidation(string? filterServiceName, string? filterAmountUsage, string? filterTotalPrice)
    {
        var notEmptyValues = new List<string>();

        if (!string.IsNullOrEmpty(filterServiceName))
        {
            notEmptyValues.Add(filterServiceName);
        }

        if (!string.IsNullOrEmpty(filterAmountUsage))
        {
            notEmptyValues.Add(filterAmountUsage);
        }

        if (!string.IsNullOrEmpty(filterTotalPrice))
        {
            notEmptyValues.Add(filterTotalPrice);
        }

        return notEmptyValues;
    }

    public async Task<List<Payment>> PaymentSearchAsync(string? filterServiceName, string? filterAmountUsageMin,
        string? filterAmountUsageMax, string? filterTotalPriceMin, string? filterTotalPriceMax)
    {
        var userId = await TakeUserIdAsync();

        // Отримання всіх платежів користувача
        var userPayments = await context.Payments
            .Include(p => p.Service)
            .Where(p => p.Service != null && p.Service.UserId == userId)
            .ToListAsync();

        if (!string.IsNullOrEmpty(filterServiceName))
        {
            userPayments = userPayments.Where(p => p.Service.ServiceName.Contains(filterServiceName)).ToList();
        }

        if (!string.IsNullOrEmpty(filterAmountUsageMin) && double.TryParse(filterAmountUsageMin, out double minUsage))
        {
            userPayments = userPayments.Where(p => p.AmountUsage >= minUsage).ToList();
        }

        if (!string.IsNullOrEmpty(filterAmountUsageMax) && double.TryParse(filterAmountUsageMax, out double maxUsage))
        {
            userPayments = userPayments.Where(p => p.AmountUsage <= maxUsage).ToList();
        }

        if (!string.IsNullOrEmpty(filterTotalPriceMin) && double.TryParse(filterTotalPriceMin, out double minPrice))
        {
            userPayments = userPayments.Where(p => p.TotalPrice >= minPrice).ToList();
        }

        if (!string.IsNullOrEmpty(filterTotalPriceMax) && double.TryParse(filterTotalPriceMax, out double maxPrice))
        {
            userPayments = userPayments.Where(p => p.TotalPrice <= maxPrice).ToList();
        }

        return userPayments;
    }

    private bool MatchesAnyParameter(Payment payment, List<string> parameters)
    {
        foreach (var parameter in parameters)
        {
            if (payment.ServiceId.ToString() == parameter)
            {
                return true;
            }

            if (payment.AmountUsage.ToString() == parameter)
            {
                return true;
            }

            if (payment.TotalPrice.ToString() == parameter)
            {
                return true;
            }

            // Assuming Date is of type string
            if (payment.Date == parameter)
            {
                return true;
            }
        }

        return false;
    }

    // Повертає відсортовані оплати
    public List<Payment> GetSortedPayments(string sortedBy, List<Payment> inputPayments)
    {
        return sortedBy switch
        {
            "Id" => inputPayments.OrderBy(payment => payment.Id).ToList(),
            "AmountUsage" => inputPayments.OrderBy(payment => payment.AmountUsage).ToList(),
            "ServiceId" => inputPayments.OrderBy(payment => payment.ServiceId).ToList(),
            "Date" => inputPayments
                .OrderBy(payment => DateTime.TryParse(payment.Date, out var date) ? date : DateTime.MinValue).ToList(),
            "TotalPrice" => inputPayments.OrderBy(payment => payment.TotalPrice).ToList(),
            _ => inputPayments
        };
    }

    public async Task<List<Service>> GetUserServicesAsync(string userId)
    {
        return await context.Services!.Where(s => s.UserId == userId).ToListAsync();
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public async Task<IActionResult> Index(string? sortedBy, string? filterServiceName, string? filterAmountUsage,
        string? filterTotalPrice, int page = 1)
    {
        var currentUser = await TakeUserIdAsync();

        var searchParameters = InputValidation(filterServiceName, filterAmountUsage, filterTotalPrice);
        var userServices = await GetUserServicesAsync(currentUser);

        List<Payment> userPayments;
        int pageSize = 8;

        if (searchParameters.Count == 0)
        {
            userPayments = await context.Payments
                .Include(p => p.Service)
                .Where(p => p.Service != null && p.Service.UserId == currentUser)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (sortedBy != null)
            {
                userPayments = GetSortedPayments(sortedBy, userPayments);
            }
        }
        else
        {
            userPayments = await PaymentSearchAsync(filterServiceName, filterAmountUsage, null, filterTotalPrice, null);
            userPayments = userPayments.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            if (sortedBy != null)
            {
                userPayments = GetSortedPayments(sortedBy, userPayments);
            }
        }

        int totalPayments =
            await context.Payments.CountAsync(p => p.Service != null && p.Service.UserId == currentUser);
        int totalPages = (int)Math.Ceiling(totalPayments / (double)pageSize);

        var model = new PaymentViewIndexModel(userPayments, userServices, page, totalPages);

        return View(model);
    }


    public IActionResult AddPayment()
    {
        var currentUser = userManager.GetUserAsync(User).Result;
        var currentUserId = currentUser.Id;

        var services = context.Services.Where(s => s.UserId == currentUserId).ToList();

        Payment newPayment = new Payment();

        return View(newPayment);
    }

    [HttpPost]
    public IActionResult AddPaymentPost(Payment newPayment)
    {
        if (ModelState.IsValid)
        {
            newPayment.Service = null;

            context.Payments.Add(newPayment);

            context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        else
        {
            return View("Index");
        }
    }

    public IActionResult DeletePayment(int id)
    {
        var payment = context.Payments.Find(id);

        if (payment == null)
        {
            return NotFound();
        }

        context.Payments.Remove(payment);
        context.SaveChangesAsync();

        return RedirectToAction("Index");
    }


    public IActionResult DetailsPayment(int id)
    {
        var payment = context.Payments.Include(p => p.Service).FirstOrDefault(p => p.Id == id);

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