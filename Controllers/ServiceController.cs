using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebKomunalka.Net8.Data;
using WebKomunalka.Net8.Models;

namespace WebKomunalka.Net8.Controllers;

public class ServiceController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public ServiceController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // Визначити, хто авторизований
        var currentUser = await _userManager.GetUserAsync(User);
    
        if (currentUser == null)
        {
            return View("NotRegistered");
        }

        var currentUserId = currentUser.Id;

        // Вибрати всі послуги, які належать авторизованому користувачу
        var vd = await _context.Services.Where(s => s.UserId == currentUserId).ToListAsync();
        
        if (vd == null)
        {
            return View("NoServices");
        }
        else
        {
            return View(vd);
        }
       
    }
    
    public IActionResult AddService()
    {
        Service newService = new Service();
        
        return View(newService);
    }
    
    public async Task<IActionResult> AddServicePost(Service newService)
    {
        if (ModelState.IsValid)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            
            var curentUserId = currentUser.Id;
            
            newService.UserId = curentUserId;
            newService.User = null;
            _context.Services.Add(newService);
            
            await _context.SaveChangesAsync();
            
            return RedirectToAction("Index");
        }
        else
        {
            return View(newService);
        }
    }
    
    public IActionResult EditService(int id)
    {
        var service = _context.Services.Find(id);
        
        return View(service);
    }

    public IActionResult EditServicePost(Service service)
    {
        if (ModelState.IsValid)
        {
            var curentService = _context.Services.Find(service.Id); // пояснення від ~ гуру ~
            if (curentService == null)
            {
                // Handle the case where the service does not exist
                return NotFound();
            }

            // Update the existing service with the data from the form
            curentService.ServiceName = service.ServiceName;
            curentService.UnitPrice = service.UnitPrice;
            curentService.UnitType = service.UnitType;
            curentService.Company = service.Company;

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        else
        {
            return View(service);
        }
    }
}