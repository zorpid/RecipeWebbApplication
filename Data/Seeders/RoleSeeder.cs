using Microsoft.AspNetCore.Identity;

namespace RecipeWebbApplication.Data.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                var role = await roleManager.FindByNameAsync(roleName);

                if (role == null) // Ensure role doesn't exist before creating
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create role: {roleName}");
                    }
                }
            }
        }

    }
}
