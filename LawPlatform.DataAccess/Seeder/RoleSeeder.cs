using LawPlatform.Entities.Models.Auth.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LawPlatform.DataAccess.Seeder
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(RoleManager<Role> _roleManager)
        {
            var rolesCount = await _roleManager.Roles.CountAsync();
            if (rolesCount <= 0)
            {
                await _roleManager.CreateAsync(new Role { Name = "Admin" });
                await _roleManager.CreateAsync(new Role { Name = "Lawyer" });
                await _roleManager.CreateAsync(new Role { Name = "Client" });
            }
        }
    }
}
