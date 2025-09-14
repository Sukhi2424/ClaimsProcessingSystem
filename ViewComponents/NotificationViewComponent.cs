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

            // This new query ONLY fetches 'Added' events for 'Claims' and 'AspNetUsers'.
            // This guarantees the 'Id' will be present and prevents the crash.
            var notifications = await _context.AuditLogs
                .Where(a => a.Action == "Added" && (a.TableName == "Claims" || a.TableName == "AspNetUsers"))
                .OrderByDescending(a => a.Timestamp)
                .Take(5)
                .ToListAsync();

            return View(notifications);
        }
    }
}