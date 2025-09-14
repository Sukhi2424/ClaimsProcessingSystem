// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using ClaimsProcessingSystem.Models.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClaimsProcessingSystem.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }
        public string CurrentUserProfilePicture { get; set; }

        [Display(Name = "Last Login")]
        public DateTime? LastLoginTime { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Display(Name = "Employee Number")]
            public string EmployeeNo { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            public IFormFile ProfilePicture { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            CurrentUserProfilePicture = user.ProfilePicturePath;
            LastLoginTime = user.LastLoginTime;

            Input = new InputModel
            {
                FullName = user.FullName,
                EmployeeNo = user.EmployeeNo,
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Update Full Name if it has changed
            if (Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
            }

            // Profile Picture Upload Logic
            if (Input.ProfilePicture != null && Input.ProfilePicture.Length > 0)
            {
                // (You can add logic here to delete the old picture if it exists)
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(Input.ProfilePicture.FileName);
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/profile-pictures");
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                Directory.CreateDirectory(uploadsFolder);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await Input.ProfilePicture.CopyToAsync(fileStream);
                }
                user.ProfilePicturePath = "/profile-pictures/" + uniqueFileName;
            }

            // Update Phone Number if it has changed
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}