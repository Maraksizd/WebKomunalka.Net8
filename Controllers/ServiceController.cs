using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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

    // check userId
    public string takeUserId()
    {
        string userId = "NotFound";

        var cUser = _userManager.GetUserAsync(User).Result;

        if (cUser == null)
        {
            userId = null;
        }

        userId = cUser.Id;

        return userId;
    }
    
    // функція для сортування
    public List<Service> SortServices(string sortedBy, List<Service> services)
    {
        switch (sortedBy)
        {
            case "ServiceName":
                return services.OrderBy(s => s.ServiceName).ToList();
            case "UnitPrice":
                return services.OrderBy(s => s.UnitPrice).ToList();
            case "UnitType":
                return services.OrderBy(s => s.UnitType).ToList();
            case "Company":
                return services.OrderBy(s => s.Company).ToList();
            default:
                return services;
        }
    }
    
    public async Task<IActionResult> Index(string? filterServiceName, double? filterUnitPrice, string? filterUnitType, string? filterCompany)
    {
        // Визначити, хто авторизований
        var currentUserId = takeUserId();

        
        var services = _context.Services.Where(s => s.UserId == currentUserId);

        // if (!string.IsNullOrEmpty(searchQuery))
        // {
        //     services = services.Where(s =>
        //         s.ServiceName.Contains(searchQuery) || 
        //         s.UnitPrice.ToString().Contains(searchQuery) ||
        //         s.UnitType.Contains(searchQuery) ||
        //         s.Company.Contains(searchQuery));
        // }

        var vd = await services.ToListAsync();

        if (vd.Count == 0)
        {
            return View("NoFoundServices", vd);
        }
        
        return View(vd);
    }


    public IActionResult AddService()
    {
        Service newService = new Service();

        return View(newService);
    }

    public async Task<IActionResult> AddServicePost(Service? newService)
    {
        if (ModelState.IsValid)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var curentUserId = currentUser!.Id;

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
            var curentService = _context.Services.Find(service.Id);
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

    public IActionResult DeleteService(int id)
    {
        var service = _context.Services.Find(id);

        if (service == null)
        {
            return NotFound();
        }

        _context.Services.Remove(service);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }
}