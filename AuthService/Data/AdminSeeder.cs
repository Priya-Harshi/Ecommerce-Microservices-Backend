using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public static class AdminSeeder
{
    public static async Task SeedAsync(
        AuthDbContext context)
    {
        if (await context.Users.AnyAsync(
            u => u.Role == "Admin"))
        {
            return;
        }

        var admin = new User
        {
            Name = "Admin",
            Email = "admin@ecommerce.com",
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    "Admin@123"),
            Role = "Admin"
        };

        context.Users.Add(admin);

        await context.SaveChangesAsync();
    }
}