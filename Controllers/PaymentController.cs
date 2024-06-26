using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using WebKomunalka.Net8.Data;
using WebKomunalka.Net8.Models;

namespace WebKomunalka.Net8.Controllers;

public class PaymentController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    : Controller
{
    // check id User
    public string takeUserId()
    {
        string userId = "";

        var cUser = userManager.GetUserAsync(User).Result;

        userId = Convert.ToString(cUser.Id);

        return userId;
    }

    // перевіряє чи поля для фільтрів/сортування не пусті
    public List<string> InputValidation(string? filterServiceName, string? filterAmountUsage,
        string? filterTotalPrice)
    {
        List<string> notEmptyValues = new List<string>();

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

    public List<Payment> PaymentSearch(List<string> parameters)
    {
        var userId = takeUserId();

        // Отримання всіх платежів користувача
        List<Payment> userPayments = context.Payments
            .Include(p => p.Service)
            .Where(p => p.Service != null && p.Service.UserId == userId)
            .ToList();

        if (parameters.Count != 0)
        {
            List<Payment> foundPayments = new List<Payment>();

            foreach (var payment in userPayments)
            {
                if (MatchesAnyParameter(payment, parameters))
                {
                    foundPayments.Add(payment);
                }
            }

            return foundPayments;
        }
        else
        {
            return userPayments;
        }
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


    // повертає відсортовані оплати
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

    public List<Service> GetUserServices(string userId)
    {
        return context.Services!.Where(s => s.UserId == userId).ToList();
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public IActionResult Index(string? sortedBy, string? filterServiceName, string? filterAmountUsage, string? filterTotalPrice, int page = 1)
    {
        var currentUser = takeUserId();

        List<string> searchParameters = InputValidation(filterServiceName, filterAmountUsage, filterTotalPrice);
        List<Service> userServices = GetUserServices(currentUser);

        List<Payment> userPayments;
        int pageSize = 8;

        if (searchParameters.Count == 0)
        {
            userPayments = context.Payments
                .Include(p => p.Service)
                .Where(p => p.Service != null && p.Service.UserId == currentUser)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (sortedBy != null)
            {
                userPayments = GetSortedPayments(sortedBy, userPayments);
            }
        }
        else
        {
            userPayments = PaymentSearch(searchParameters)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (sortedBy != null)
            {
                userPayments = GetSortedPayments(sortedBy, userPayments);
            }
        }

        int totalPayments = context.Payments.Count(p => p.Service != null && p.Service.UserId == currentUser);
        int totalPages = (int)Math.Ceiling(totalPayments / (double)pageSize);

        PaymentViewIndexModel model = new PaymentViewIndexModel(userPayments, userServices, page, totalPages);

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