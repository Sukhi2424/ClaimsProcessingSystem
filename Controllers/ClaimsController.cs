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
        public async Task<IActionResult> Index(string statusFilter)
        {
            ViewData["CurrentFilter"] = statusFilter;

            var currentUserId = _userManager.GetUserId(User);
            var claimsQuery = _context.Claims
                                      .Where(c => c.SubmittingUserId == currentUserId)
                                      .AsQueryable();

            // Filtering Logic
            if (!String.IsNullOrEmpty(statusFilter))
            {
                if (Enum.TryParse<ClaimStatus>(statusFilter, out var status))
                {
                    claimsQuery = claimsQuery.Where(c => c.Status == status);
                }
            }

            var userClaims = await claimsQuery.OrderByDescending(c => c.DateSubmitted).ToListAsync();

            // The badge count should reflect the filtered list for the user
            ViewBag.PendingCount = userClaims.Count(c => c.Status == ClaimStatus.Pending);

            return View(userClaims);
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
            return View(claim);
        }

        // POST: Claims/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,RequestedAmount,SubmittingUserId")] Claim claim, IFormFile? newSupportingDocument, bool removeExistingDocument)
        {
            if (id != claim.Id)
            {
                return NotFound();
            }

            // We must fetch the original claim from DB to get all its properties
            var claimToUpdate = await _context.Claims.FindAsync(id);
            if (claimToUpdate == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update the properties from the form
                claimToUpdate.Title = claim.Title;
                claimToUpdate.Description = claim.Description;
                claimToUpdate.RequestedAmount = claim.RequestedAmount;

                // --- FILE MANAGEMENT LOGIC ---
                // 1. Check if user wants to remove the existing document
                if (removeExistingDocument && !string.IsNullOrEmpty(claimToUpdate.SupportingDocumentPath))
                {
                    // (Optional but good practice) Delete the old file from the server
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", claimToUpdate.SupportingDocumentPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                    claimToUpdate.SupportingDocumentPath = null;
                }

                // 2. Check if a new document is being uploaded
                if (newSupportingDocument != null && newSupportingDocument.Length > 0)
                {
                    // Delete the old file first if it exists
                    if (!string.IsNullOrEmpty(claimToUpdate.SupportingDocumentPath))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", claimToUpdate.SupportingDocumentPath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Save the new file
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(newSupportingDocument.FileName);
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    var newFilePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                    {
                        await newSupportingDocument.CopyToAsync(fileStream);
                    }
                    claimToUpdate.SupportingDocumentPath = "/uploads/" + uniqueFileName;
                }
                // --- END FILE LOGIC ---

                try
                {
                    _context.Update(claimToUpdate);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Your claim has been updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency error if needed
                    throw;
                }
                return RedirectToAction(nameof(Index));
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
