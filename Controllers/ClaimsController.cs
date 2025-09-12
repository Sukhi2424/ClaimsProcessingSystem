using ClaimsProcessingSystem.Data;
using ClaimsProcessingSystem.Models;
using ClaimsProcessingSystem.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClaimsProcessingSystem.Helpers;

namespace ClaimsProcessingSystem.Controllers
{
    [Authorize]
    public class ClaimsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        public ClaimsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Claims
        public async Task<IActionResult> Index(string statusFilter, int pageNumber = 1)
        {
            ViewData["CurrentFilter"] = statusFilter;
            var currentUserId = _userManager.GetUserId(User);

            var claimsQuery = _context.Claims
                                      .Where(c => c.SubmittingUserId == currentUserId)
                                      .AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<ClaimStatus>(statusFilter, out var status))
            {
                claimsQuery = claimsQuery.Where(c => c.Status == status);
            }

            int pageSize = 10;
            var paginatedClaims = await PaginatedList<Claim>.CreateAsync(claimsQuery.OrderByDescending(c => c.DateSubmitted), pageNumber, pageSize);

            return View(paginatedClaims);
        }

        // GET: Claims/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _context.Claims
                .FirstOrDefaultAsync(m => m.Id == id);
            if (claim == null)
            {
                return NotFound();
            }

            return View(claim);
        }

        // GET: Claims/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Claims/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,RequestedAmount")] Claim claim, IFormFile? supportingDocument)
        {
            if (ModelState.IsValid)
            {
                // --- FILE UPLOAD LOGIC ---
                if (supportingDocument != null && supportingDocument.Length > 0)
                {
                    // Create a unique file name to avoid conflicts
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(supportingDocument.FileName);

                    // Define the path to save the file in wwwroot/uploads
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Create the directory if it doesn't exist
                    Directory.CreateDirectory(uploadsFolder);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await supportingDocument.CopyToAsync(fileStream);
                    }

                    // Store the file path in the model (relative to wwwroot)
                    claim.SupportingDocumentPath = "/uploads/" + uniqueFileName;
                }
                // --- END OF FILE UPLOAD LOGIC ---

                var userId = _userManager.GetUserId(User);
                claim.SubmittingUserId = userId;
                claim.DateSubmitted = DateTime.Now;
                claim.Status = ClaimStatus.Pending;

                _context.Add(claim);
                await _context.SaveChangesAsync();

                TempData["success"] = "Your claim has been submitted successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(claim);
        }

        // GET: Claims/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
            }

            // ADD THIS SECURITY CHECK
            if (claim.Status != ClaimStatus.Pending)
            {
                TempData["error"] = "This claim has already been processed and cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            return View(claim);
        }

        // POST: Claims/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,SubmittingUserId")] Claim claim, IFormFile? newSupportingDocument, bool removeExistingDocument)
        {
            if (id != claim.Id)
            {
                return NotFound();
            }

            var claimToUpdate = await _context.Claims.FindAsync(id);
            if (claimToUpdate == null)
            {
                return NotFound();
            }

            // ADD THIS SECURITY CHECK
            if (claimToUpdate.Status != ClaimStatus.Pending)
            {
                TempData["error"] = "This claim has already been processed and cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                // (The rest of the method logic remains the same)
                // ...
            }
            return View(claim);
        }

        // GET: Claims/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _context.Claims
                .FirstOrDefaultAsync(m => m.Id == id);
            if (claim == null)
            {
                return NotFound();
            }

            return View(claim);
        }

        // POST: Claims/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                _context.Claims.Remove(claim);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClaimExists(int id)
        {
            return _context.Claims.Any(e => e.Id == id);
        }
    }
}
