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
        public async Task<IActionResult> Index(string sortOrder, string statusFilter, int pageNumber = 1, string searchString = null)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = statusFilter;
            ViewData["CurrentSearch"] = searchString; // Pass search string back to view

            var claimsQuery = _context.Claims.Include(c => c.SubmittingUser).AsQueryable();

            // Search Logic
            if (!string.IsNullOrEmpty(searchString))
            {
                claimsQuery = claimsQuery.Where(c => c.Title.Contains(searchString)
                                                  || c.SubmittingUser.FullName.Contains(searchString));
            }

            // Filtering Logic
            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<ClaimStatus>(statusFilter, out var status))
            {
                claimsQuery = claimsQuery.Where(c => c.Status == status);
            }

            // Sorting Logic...
            claimsQuery = claimsQuery.OrderByDescending(c => c.DateSubmitted);

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
                claim.DateProcessed = DateTime.UtcNow; 
                await _context.SaveChangesAsync();

                var subject = $"Your Claim '{claim.Title}' has been Approved";
                var message = $"Dear {claim.SubmittingUser.FullName},<br><br>Your claim for {claim.RequestedAmount.ToString("C", new CultureInfo("en-IN"))} has been approved for the amount of {claim.ApprovedAmount?.ToString("C", new CultureInfo("en-IN"))}.<br><br>Thank you,<br>ClaimsPro System";
                _= _emailSender.SendEmailAsync(claim.SubmittingUser.Email, subject, message);

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
                claim.DateProcessed = DateTime.UtcNow; 
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
        // GET: Admin/Reports
        public async Task<IActionResult> Reports()
        {
            ViewBag.ApprovedCount = await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Approved);
            ViewBag.RejectedCount = await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Rejected);
            ViewBag.PendingCount = await _context.Claims.CountAsync(c => c.Status == ClaimStatus.Pending);

            return View();
        }
    }
}