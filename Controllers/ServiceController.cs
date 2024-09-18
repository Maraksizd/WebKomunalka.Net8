using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebKomunalka.Net8.Data;
using WebKomunalka.Net8.Models;

namespace WebKomunalka.Net8.Controllers
{
    public class ServiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ServiceController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        
        public bool IsUserLogined()
        {
            return _userManager.GetUserAsync(User).Result != null;
        }
        
        
        // Get the current user ID
        private async Task<string> TakeUserIdAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            return currentUser?.Id ?? "NotFound";
        }

        // Function to sort services based on the sorting criteria
        private List<Service> SortServices(string sortedBy, List<Service> services)
        {
            return sortedBy switch
            {
                "ServiceName" => services.OrderBy(s => s.ServiceName).ToList(),
                "UnitPrice" => services.OrderBy(s => s.UnitPrice).ToList(),
                "UnitType" => services.OrderBy(s => s.UnitType).ToList(),
                "Company" => services.OrderBy(s => s.Company).ToList(),
                _ => services
            };
        }

        public List<Service> ServicesSearch(List<Service> allServices, List<string> inputParameters)
        {
            List<Service> trueServices = new List<Service>();

            foreach (var service in allServices)
            {
                if (inputParameters.Contains(service.ServiceName) ||
                    inputParameters.Contains(service.UnitPrice.ToString()) ||
                    inputParameters.Contains(service.UnitType) || inputParameters.Contains(service.Company))
                {
                    trueServices.Add(service);
                }
            }

            return trueServices;
        }

        public List<string> CheckNonNullable(string? filterServiceName, double? filterUnitPriceMin,
            double? filterUnitPriceMax, string? filterUnitType, string? filterCompany, string? sortedBy)
        {
            List<string> nonNullable = new List<string>();

            if (!string.IsNullOrEmpty(filterServiceName))
                nonNullable.Add(filterServiceName);

            if (filterUnitPriceMin.HasValue)
                nonNullable.Add(filterUnitPriceMin.Value.ToString());

            if (filterUnitPriceMax.HasValue)
                nonNullable.Add(filterUnitPriceMax.Value.ToString());

            if (!string.IsNullOrEmpty(filterUnitType))
                nonNullable.Add(filterUnitType);

            if (!string.IsNullOrEmpty(filterCompany))
                nonNullable.Add(filterCompany);

            if (!string.IsNullOrEmpty(sortedBy))
                nonNullable.Add(sortedBy);

            return nonNullable;
        }

        // Index action to list services with filtering and sorting
        public async Task<IActionResult> Index(string? filterServiceName, double? filterUnitPriceMin,
            double? filterUnitPriceMax, string? filterUnitType, string? filterCompany, string? sortedBy, int page = 1)
        {
            
            if (!IsUserLogined())
            {
                return RedirectToAction("Index", "Home");
            }
            
            var currentUserId = await TakeUserIdAsync();
            var currentUserServices = await _context.Services.Where(s => s.UserId == currentUserId).ToListAsync();

            ServiceViewIndexModel viewModel = new ServiceViewIndexModel
            {
                TotalPages = (int)Math.Ceiling(currentUserServices.Count / 10.0),
                CurrentPage = page
            };

            var nonNullable = CheckNonNullable(filterServiceName, filterUnitPriceMin, filterUnitPriceMax,
                filterUnitType, filterCompany, sortedBy);

            if (nonNullable.Count == 0)
            {
                viewModel.Services = currentUserServices.Skip((page - 1) * 10).Take(10).ToList();
            }
            else
            {
                var foundServices = ServicesSearch(currentUserServices, nonNullable);

                if (!string.IsNullOrEmpty(sortedBy))
                {
                    foundServices = SortServices(sortedBy, foundServices);
                }

                viewModel.Services = foundServices.Skip((page - 1) * 10).Take(10).ToList();
            }

            return View(viewModel);
        }


        public IActionResult AddService()
        {
            return View(new Service());
        }

        [HttpPost]
        public async Task<IActionResult> AddService(Service newService)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                newService.UserId = currentUser.Id;
                _context.Services.Add(newService);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(newService);
        }

        public async Task<IActionResult> EditService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return NotFound();

            return View(service);
        }

        [HttpPost]
        public async Task<IActionResult> EditService(Service service)
        {
            if (ModelState.IsValid)
            {
                var existingService = await _context.Services.FindAsync(service.Id);
                if (existingService == null)
                    return NotFound();

                existingService.ServiceName = service.ServiceName;
                existingService.UnitPrice = service.UnitPrice;
                existingService.UnitType = service.UnitType;
                existingService.Company = service.Company;

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(service);
        }

        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
                return NotFound();

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}