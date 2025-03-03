using Microsoft.AspNetCore.Identity;
using RecipeWebbApplication.Models;

namespace RecipeWebbApplication.Data.Seeders
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminUser(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string adminEmail = "admin@admin.com";
            string adminPassword = "Admin!123"; // Change this!
            string FullName = "Admin User"; // ✅ Provide a valid value

            // Ensure the Admin role exists
            string adminRole = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            // Ensure the Admin user exists
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                await userManager.CreateAsync(adminUser, adminPassword);
            }

            // Assign Admin role if not already assigned
            if (!await userManager.IsInRoleAsync(adminUser, adminRole))
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
        }
    }

}
