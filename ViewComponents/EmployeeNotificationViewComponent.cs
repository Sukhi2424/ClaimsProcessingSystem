using ClaimsProcessingSystem.Data;
using ClaimsProcessingSystem.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ClaimsProcessingSystem.ViewComponents
{
    public class EmployeeNotificationViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeNotificationViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _userManager.GetUserId(HttpContext.User);

            var notifications = await _context.Claims
    .Where(c => c.SubmittingUserId == userId && (c.Status == Models.ClaimStatus.Approved || c.Status == Models.ClaimStatus.Rejected))
    .OrderByDescending(c => c.DateProcessed)
    .ToListAsync();

            return View(notifications);
        }
    }
}