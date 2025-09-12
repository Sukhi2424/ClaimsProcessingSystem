using ClaimsProcessingSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ClaimsProcessingSystem.ViewComponents
{
    public class NotificationViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public NotificationViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Fetch the top 5 latest "Added" actions from the audit log
            var notifications = await _context.AuditLogs
    .Where(a => a.Action == "Added")
    .OrderByDescending(a => a.Timestamp)
    .ToListAsync();

            return View(notifications);
        }
    }
}