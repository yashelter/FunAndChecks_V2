using FunAndChecks.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FunAndChecks.Data.Seeding
{
    public class DataSeeder
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(UserManager<User> userManager, RoleManager<Role> roleManager, IConfiguration configuration, ILogger<DataSeeder> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await SeedRolesAsync();
            await SeedAdminUsersAsync();
        }

        private async Task SeedRolesAsync()
        {
            if (await _roleManager.FindByNameAsync("Admin") == null)
            {
                await _roleManager.CreateAsync(new Role { Name = "Admin" });
                _logger.LogInformation("Role 'Admin' created.");
            }

            if (await _roleManager.FindByNameAsync("User") == null)
            {
                await _roleManager.CreateAsync(new Role { Name = "User" });
                _logger.LogInformation("Role 'User' created.");
            }
        }

        private async Task SeedAdminUsersAsync()
        {
            var adminsToCreate = _configuration.GetSection("InitialAdmins").Get<List<AdminSeedModel>>();

            if (adminsToCreate == null || !adminsToCreate.Any())
            {
                _logger.LogWarning("No initial admins found in configuration. Skipping admin seeding.");
                return;
            }

            foreach (var adminModel in adminsToCreate)
            {
                if (await _userManager.FindByEmailAsync(adminModel.Email) != null)
                {
                    _logger.LogInformation("Admin with email {Email} already exists. Skipping.", adminModel.Email);
                    continue;
                }

                var adminUser = new User
                {
                    FirstName = adminModel.FirstName,
                    LastName = adminModel.LastName,
                    Email = adminModel.Email,
                    UserName = adminModel.TgUsername ,
                    TelegramUserId = adminModel.TgId,
                    Color = adminModel.Color,
                    
                    EmailConfirmed = true,
                    GroupId = null,
                    GitHubUrl = null,
                };

                var result = await _userManager.CreateAsync(adminUser, adminModel.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("Admin user {Email} created successfully and assigned 'Admin' role.", adminModel.Email);
                }
                else
                {
                    _logger.LogError("Failed to create admin user {Email}. Errors: {Errors}", 
                        adminModel.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}