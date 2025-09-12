using ClaimsProcessingSystem.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using System.Security.Claims;
using ClaimsProcessingSystem.Data;
using ClaimsProcessingSystem.Helpers;

namespace ClaimsProcessingSystem.Controllers
{
    [Authorize(Roles = "Manager")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index(int pageNumber = 1, string searchString = null)
        {
            ViewData["CurrentSearch"] = searchString;
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u => u.FullName.Contains(searchString)
                                                || u.Email.Contains(searchString));
            }

            int pageSize = 10;
            var paginatedUsers = await PaginatedList<ApplicationUser>.CreateAsync(usersQuery.OrderBy(u => u.FullName), pageNumber, pageSize);
            return View(paginatedUsers);
        }
        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser appUser)
        {
            if (id != appUser.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    user.FullName = appUser.FullName;
                    user.EmployeeNo = appUser.EmployeeNo;
                    user.Email = appUser.Email;
                    user.UserName = appUser.Email; // Keep username in sync with email
                    user.NormalizedEmail = appUser.Email.ToUpper();
                    user.NormalizedUserName = appUser.Email.ToUpper();

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(appUser);
        }
        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Safety check: prevent admin from deleting their own account
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == currentUserId)
            {
                TempData["error"] = "Error: You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // Safety check: prevent admin from deleting their own account
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == currentUserId)
            {
                TempData["error"] = "Error: You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Important: First, delete all claims associated with this user
                var userClaims = _context.Claims.Where(c => c.SubmittingUserId == id);
                _context.Claims.RemoveRange(userClaims);
                await _context.SaveChangesAsync();

                // Now, delete the user
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["success"] = "User has been deleted successfully.";
                    return RedirectToAction(nameof(Index));
                }

                // If something went wrong, add errors
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(user);
        }
    }
}