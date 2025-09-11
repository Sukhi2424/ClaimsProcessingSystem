using ClaimsProcessingSystem.Data;
using ClaimsProcessingSystem.Models;
using ClaimsProcessingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ClaimsProcessingSystem.Helpers;

namespace ClaimsProcessingSystem.Controllers
{
    [Authorize(Roles = "Manager")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailSender _emailSender;

        public AdminController(ApplicationDbContext context, EmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: Admin or Admin/Index
        public async Task<IActionResult> Index(string sortOrder, string statusFilter, int pageNumber = 1)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["AmountSortParm"] = String.IsNullOrEmpty(sortOrder) ? "amount_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentFilter"] = statusFilter;

            var claimsQuery = _context.Claims.Include(c => c.SubmittingUser).AsQueryable();

            // Filtering Logic...
            if (!String.IsNullOrEmpty(statusFilter))
            {
                if (Enum.TryParse<ClaimStatus>(statusFilter, out var status))
                {
                    claimsQuery = claimsQuery.Where(c => c.Status == status);
                }
            }

            // Sorting Logic...
            switch (sortOrder)
            {
                case "amount_desc":
                    claimsQuery = claimsQuery.OrderByDescending(c => c.RequestedAmount);
                    break;
                case "Date":
                    claimsQuery = claimsQuery.OrderBy(c => c.DateSubmitted);
                    break;
                case "date_desc":
                    claimsQuery = claimsQuery.OrderByDescending(c => c.DateSubmitted);
                    break;
                default:
                    claimsQuery = claimsQuery.OrderBy(c => c.DateSubmitted).Reverse(); // Default sort by most recent
                    break;
            }

            int pageSize = 10;
            var paginatedClaims = await PaginatedList<Claim>.CreateAsync(claimsQuery, pageNumber, pageSize);

            ViewBag.PendingCount = await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Pending);

            return View(paginatedClaims);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClaim(int id)
        {
            const decimal maxReimbursement = 50000.00m;
            var claim = await _context.Claims.Include(c => c.SubmittingUser).FirstOrDefaultAsync(c => c.Id == id);

            if (claim != null)
            {
                var approvedAmount = Math.Min(claim.RequestedAmount, maxReimbursement);
                claim.ApprovedAmount = approvedAmount;
                claim.Status = Models.ClaimStatus.Approved;
                await _context.SaveChangesAsync();

                var subject = $"Your Claim '{claim.Title}' has been Approved";
                var message = $"Dear {claim.SubmittingUser.FullName},<br><br>Your claim for {claim.RequestedAmount.ToString("C", new CultureInfo("en-IN"))} has been approved for the amount of {claim.ApprovedAmount?.ToString("C", new CultureInfo("en-IN"))}.<br><br>Thank you,<br>ClaimsPro System";
                await _emailSender.SendEmailAsync(claim.SubmittingUser.Email, subject, message);

                TempData["success"] = "Claim approved and notification sent.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(int id)
        {
            var claim = await _context.Claims.Include(c => c.SubmittingUser).FirstOrDefaultAsync(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = Models.ClaimStatus.Rejected;
                await _context.SaveChangesAsync();

                var subject = $"Update on Your Claim '{claim.Title}'";
                var message = $"Dear {claim.SubmittingUser.FullName},<br><br>We regret to inform you that your claim for {claim.RequestedAmount.ToString("C", new CultureInfo("en-IN"))} has been rejected.<br><br>Thank you,<br>ClaimsPro System";
                await _emailSender.SendEmailAsync(claim.SubmittingUser.Email, subject, message);

                TempData["warning"] = "Claim rejected and notification sent.";
            }
            return RedirectToAction(nameof(Index));
        }
        // GET: Admin/AllActivity
        public async Task<IActionResult> AllActivity()
        {
            var allLogs = await _context.AuditLogs
                                        .OrderByDescending(a => a.Timestamp)
                                        .ToListAsync();
            return View(allLogs);
        }
    }
}