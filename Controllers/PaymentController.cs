using System;
using System.Collections.Generic;
using System.Linq;
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


    // check id User
    public string takeUserId()
    {
        string userId = "";

        var cUser = _userManager.GetUserAsync(User).Result;

        userId = Convert.ToString(cUser.Id);

        return userId;
    }

    // перевіряє чи поля для фільтрів/сортування не пусті
    public List<string> InputValidation(string? sortedBy, string? filterServiceName, string? filterAmountUsage,
        string? filterTotalPrice)
    {
        List<string> notEmptyValues = new List<string>();

        if (sortedBy != null)
        {
            notEmptyValues.Add(sortedBy);
        }

        if (filterServiceName != null)
        {
            notEmptyValues.Add(filterServiceName);
        }

        if (filterAmountUsage != null)
        {
            notEmptyValues.Add(filterAmountUsage);
        }

        if (filterTotalPrice != null)
        {
            notEmptyValues.Add(filterTotalPrice);
        }

        return notEmptyValues;
    }

    // повертає відфільтровані оплати
    public List<Payment> PaymentSearch(List<string> parameters)
    {
        var cUser = _userManager.GetUserAsync(User).Result;
        var currentUserId = cUser.Id;

        // Start with all payments related to the current user's services
        var query = _context.Payments
            .Include(p => p.Service)
            .Where(p => p.Service != null && p.Service.UserId == currentUserId);

        // Apply filters based on the parameters
        foreach (var param in parameters)
        {
            switch (param.ToLower())
            {
                case"Id":
                    query = query.Where(p => p.Id != null);
                    break;
                case "service name":
                    // Assuming you have a 'ServiceName' property on the Service entity
                    query = query.Where(p => p.Service.ServiceName != null);
                    break;
                case "amount usage":
                    // Assuming you have an 'AmountUsage' property on the Payment entity
                    query = query.Where(p => p.AmountUsage != null);
                    break;
                case "total price":
                    // Assuming you have a 'TotalPrice' property on the Payment entity
                    query = query.Where(p => p.TotalPrice != null);
                    break;
                default:
                    // Handle unknown parameters or ignore them
                    break;
            }
        }

        // Execute the query and return the results
        List<Payment> payments = query.ToList();
        return payments;
    }


    // повертає відсортовані оплати
    public List<Payment> GetSortedPayments(string sortedBy, List<Payment> inputPayments)
    {
        switch (sortedBy)
        {
            case "Id":
                return inputPayments.OrderBy(payment => payment.Id).ToList();
            case "AmountUsage":
                return inputPayments.OrderBy(payment => payment.AmountUsage).ToList();
            case "ServiceId":
                return inputPayments.OrderBy(payment => payment.ServiceId).ToList();
            case "Date":
                // Assuming 'Date' is a string representation of date, you might need to parse it first.
                return inputPayments.OrderBy(payment => DateTime.Parse(payment.Date)).ToList();
            case "TotalPrice":
                return inputPayments.OrderBy(payment => payment.TotalPrice).ToList();
            // Add other sorting options as needed
            default:
                return inputPayments;
        }
    }


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public IActionResult Index(string? sortedBy, string? filterServiceName, string? filterAmountUsage,
        string? filterTotalPrice)
    {
        // Опції для сортування
        List<string> sortByOptions = new List<string> { "Amount Usage", "Total Price" };

        // Отримання ідентифікатора поточного користувача
        var userId = takeUserId(); // Припускається, що ця функція повертає ідентифікатор поточного користувача

        // Отримання списку послуг поточного користувача
        List<Service> userServices = _context.Services
            .Where(s => s.UserId == userId)
            .ToList();

        // Валідація та підготовка вхідних параметрів для фільтрації
        List<string> inputParameters =
            InputValidation(sortedBy, filterServiceName, filterAmountUsage, filterTotalPrice);

        // Пошук платежів за вказаними параметрами
        List<Payment> filteredPayments = PaymentSearch(inputParameters);

        ViewBag.Services = userServices;
        ViewBag.searchParameters = inputParameters;
        ViewBag.SortBy = sortByOptions;

        // Логіка відображення відсортованих або несортованих платежів
        // Логіка відображення відсортованих або несортованих платежів
        if (!(inputParameters != null || inputParameters.Count == 0))
        {
            // Якщо список параметрів фільтрації порожній, відображати невідсортовані платежі
            PaymentViewIndexModel model = new PaymentViewIndexModel(filteredPayments, userServices);
            return View(model);
        }
        else
        {
            // Якщо в списку є параметри фільтрації, сортувати платежі і відображати
            List<Payment> sortedPayments = GetSortedPayments(sortedBy, filteredPayments);
            PaymentViewIndexModel model = new PaymentViewIndexModel(sortedPayments, userServices);
            return View(model);
        }

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