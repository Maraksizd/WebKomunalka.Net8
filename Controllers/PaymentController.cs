using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebKomunalka.Net8.Data;
using WebKomunalka.Net8.Models;

namespace WebKomunalka.Net8.Controllers;

public class PaymentController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    : Controller
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<IdentityUser> _userManager = userManager;
    
    public IActionResult Index()
    {
        var cUser = _userManager.GetUserAsync(User).Result;

        if (cUser == null)
        {
            return View("NotRegistered");
        }

        var currentUserId = cUser.Id;

        var lpay = _context.Payments.Include(p => p.Service).Where(p => p.Service != null && p.Service.UserId == currentUserId).ToListAsync().Result;
            

        return View(lpay);
    }
    
   public IActionResult AddPayment()
{
    var currentUser = _userManager.GetUserAsync(User).Result;
    var currentUserId = currentUser.Id;

    var services = _context.Services.Where(s => s.UserId == currentUserId).ToList();
    ViewBag.Services = new SelectList(services, "Id", "ServiceName");

    Payment newPayment = new Payment();

    return View(newPayment);
}
    
    [HttpPost]
    public IActionResult AddPaymentPost(Payment newPayment)
    {
        if (ModelState.IsValid)
        {
            var currentUser = _userManager.GetUserAsync(User).Result;

            if (currentUser != null)
            {
                var curentUserId = currentUser.Id;
            
                newPayment.Service = _context.Services.FirstOrDefault(s => s != null && s.UserId == curentUserId);
            }

            _context.Payments.Add(newPayment);
            
            _context.SaveChanges();
            
            return RedirectToAction("Index");
        }
        else
        {
            return View("Index");
        }
    }
}