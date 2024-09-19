using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebKomunalka.Net8.Data;
using WebKomunalka.Net8.Models;

namespace WebKomunalka.Net8.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        ILogger<HomeController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return View("IndexNotReg");
        } // not registered

        var currentUserId = currentUser.Id;

        var allServices = await _context.Services.Where(s => s.UserId == currentUserId).ToListAsync(); // ліст всіх сервісів користувача

        var allPayments = await _context.Payments.Include(p => p.Service).Where(p => p.Service.UserId == currentUserId).ToListAsync(); // ліст всіх платежів користувача

        if (!allPayments.Any())
        {
            return View("IndexNoPayments");
        } // no payments

        double totalCost = allPayments.Sum(p => p.TotalPrice);

        var bigPayments = allPayments
            .OrderByDescending(p => p.TotalPrice)
            .Take(3)
            .Select(p => new HomeViewModel.BigPayment
            {
                Payment = p,
                Percentage = (p.TotalPrice / totalCost) * 100
            })
            .ToList();

        var model = new HomeViewModel { TotalCost = totalCost, BigPayments = bigPayments };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}