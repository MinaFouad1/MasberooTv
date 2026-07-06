using Microsoft.AspNetCore.Identity;

namespace MassperoTVAPI.Extensions;

public static class RoleSeeder
{
    /// <summary>
    /// Ensures the Admin and Company roles exist in the database.
    /// Called once on application startup.
    /// </summary>
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = ["Admin", "HR", "Client"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}
