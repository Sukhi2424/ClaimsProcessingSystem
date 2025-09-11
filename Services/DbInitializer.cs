using ClaimsProcessingSystem.Data;
using Microsoft.AspNetCore.Identity;

namespace ClaimsProcessingSystem.Services
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>(); // Add logger

            // Add roles if they don't exist
            string[] roleNames = { "Manager", "Employee" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create a default admin user if one doesn't exist
            var adminEmail = "admin@test.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var newAdminUser = new IdentityUser()
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(newAdminUser, "Password123!");

                if (result.Succeeded)
                {
                    // Assign the "Manager" role to the new user
                    await userManager.AddToRoleAsync(newAdminUser, "Manager");
                    logger.LogInformation("Admin user created successfully."); // Success message
                }
                else
                {
                    // --- THIS IS THE NEW PART ---
                    // If creation fails, log each error
                    foreach (var error in result.Errors)
                    {
                        logger.LogError($"Error creating admin user: {error.Description}");
                    }
                    // --------------------------
                }
            }
        }
    }
}