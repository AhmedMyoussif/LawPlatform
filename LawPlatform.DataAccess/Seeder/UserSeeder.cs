using LawPlatform.Entities.Models.Auth.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LawPlatform.DataAccess.Seeder
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(UserManager<User> _userManager)
        {
            var usersCount = await _userManager.Users.CountAsync();
            if (usersCount <= 0)
            {
                var adminUser = new User()
                {
                    UserName = "admin",
                    Email = "admin@gmail.com",
                    PhoneNumber = "01555803091",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(adminUser, "P@ssw0rd123Pass");
                await _userManager.AddToRoleAsync(adminUser, "Admin");

                var lawyerUser = new User()
                {
                    UserName = "ahmedYoussefLawyer",
                    Email = "ahmedyoussef@gmail.com",
                    PhoneNumber = "01069391275",
                    EmailConfirmed = true,
                };
                await _userManager.CreateAsync(lawyerUser, "P@ssw0rd123Pass");
                await _userManager.AddToRoleAsync(lawyerUser, "Lawyer");
                
                
                var clientUser = new User()
                {
                    UserName = "ziadMohammedClient",
                    Email = "ziadmohammed@gmail.com",
                    PhoneNumber = "01224309198",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(clientUser, "P@ssw0rd123Pass");
                await _userManager.AddToRoleAsync(clientUser, "Client");
            }
        }
    }
}
