using ClaimsProcessingSystem.Data;
using ClaimsProcessingSystem.Models.Identity;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ClaimsProcessingSystem.Services
{
    public static class DbInitializer
    {
        public static async Task Initialize(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Add roles if they don't exist
            string[] roleNames = { "Manager", "Employee" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create a default admin user if one doesn't exist
            var adminEmail = "admin@test.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Admin User",
                    EmployeeNo = "ADMIN001"
                };

                var result = await userManager.CreateAsync(adminUser, "Password123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Manager");
                }
            }
        }
    }
}