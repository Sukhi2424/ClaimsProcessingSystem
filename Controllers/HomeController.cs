using ClaimsProcessingSystem.Data;
using ClaimsProcessingSystem.Models;
using ClaimsProcessingSystem.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ClaimsProcessingSystem.Controllers
{
    // ViewModels for our two different dashboards
    public class EmployeeDashboardViewModel
    {
        public string FullName { get; set; }
        public int PendingClaimsCount { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int TotalPendingClaims { get; set; }
        public int TotalUsers { get; set; }
    }


    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Check if the user is in the "Manager" role
                if (User.IsInRole("Manager"))
                {
                    var adminViewModel = new AdminDashboardViewModel
                    {
                        TotalPendingClaims = _context.Claims.Count(c => c.Status == ClaimStatus.Pending),
                        TotalUsers = _userManager.Users.Count()
                    };
                    // Return a specific view for the Admin
                    return View("AdminDashboard", adminViewModel);
                }
                else // Otherwise, the user is a regular employee
                {
                    var user = await _userManager.GetUserAsync(User);
                    var claims = _context.Claims
                                         .Where(c => c.SubmittingUserId == user.Id && c.Status == ClaimStatus.Pending);

                    var employeeViewModel = new EmployeeDashboardViewModel
                    {
                        FullName = user.FullName,
                        PendingClaimsCount = claims.Count()
                    };
                    // Return the default view for the Employee
                    return View(employeeViewModel);
                }
            }

            // If user is not logged in, show the default homepage
            return View();
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
}