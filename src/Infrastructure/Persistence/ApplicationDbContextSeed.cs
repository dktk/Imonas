using System.Reflection;

using Application.Common.Extensions;
using Application.Common.Interfaces;

using Infrastructure.Constants.Permission;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public static class ApplicationDbContextSeed
    {
        public static async Task SeedDefaultUserAsync(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IDateTime dateTimeService)
        {
            var rolesCount = await roleManager.Roles.CountAsync();

            if (rolesCount > 0)
            {
                return;
            }

            var administratorRole = new ApplicationRole("Admin") { Description = "Admin Group" };
            var userRole = new ApplicationRole("Basic") { Description = "Basic Group" };

            if (roleManager.Roles.All(r => r.Name != administratorRole.Name))
            {
                await roleManager.CreateAsync(administratorRole);
                await roleManager.CreateAsync(userRole);
                var Permissions = GetAllPermissions();
                foreach (var permission in Permissions)
                {
                    await roleManager.AddClaimAsync(administratorRole, new System.Security.Claims.Claim(ApplicationClaimTypes.Permission, permission));
                }
            }

            var administrator = new ApplicationUser { UserName = "administrator", IsActive = true, Site = "Razor", DisplayName = "Administrator", Email = "new163@163.com", EmailConfirmed = true, ProfilePictureDataUrl = $"https://avatars.githubusercontent.com/u/5129897?v=4&size=64" };
            var calin = new ApplicationUser { UserName = "calin", IsActive = true, Site = "Razor", DisplayName = "calin", Email = "new163@163.com", EmailConfirmed = true, ProfilePictureDataUrl = $"https://avatars.githubusercontent.com/u/5129897?v=4&size=64" };
            var daniela = new ApplicationUser { UserName = "daniela", IsActive = true, Site = "Razor", DisplayName = "Daniela", Email = "new163@163.com", EmailConfirmed = true, ProfilePictureDataUrl = $"https://avatars.githubusercontent.com/u/5129897?v=4&size=64" };

            var test1 = new ApplicationUser { UserName = "test1", IsActive = true, Site = "Razor", DisplayName = "test1", Email = "test1@126.com", EmailConfirmed = true, ProfilePictureDataUrl = $"https://avatars.githubusercontent.com/u/5129897?v=4&size=64" };
            var test2 = new ApplicationUser { UserName = "test2", IsActive = true, Site = "Razor", DisplayName = "test2", Email = "test1@126.com", EmailConfirmed = true, ProfilePictureDataUrl = $"https://avatars.githubusercontent.com/u/5129897?v=4&size=64" };

            if (userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                await userManager.CreateAsync(administrator, "Password123!");
                await userManager.AddToRolesAsync(administrator, new[] { administratorRole.Name });

                await userManager.CreateAsync(calin, "Password123!");
                await userManager.AddToRolesAsync(calin, new[] { administratorRole.Name });

                await userManager.CreateAsync(daniela, "Password123!");
                await userManager.AddToRolesAsync(daniela, new[] { administratorRole.Name });

                await userManager.CreateAsync(test1, "Password123!");
                await userManager.AddToRolesAsync(test1, new[] { userRole.Name });

                await userManager.CreateAsync(test2, "Password123!");
                await userManager.AddToRolesAsync(test2, new[] { userRole.Name });
            }
        }

        private static IEnumerable<string> GetAllPermissions()
        {
            var allPermissions = new List<string>();
            var modules = typeof(Permissions).GetNestedTypes();

            foreach (var module in modules)
            {
                var moduleName = string.Empty;
                var moduleDescription = string.Empty;

                var fields = module.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                foreach (var fi in fields)
                {
                    var propertyValue = fi.GetValue(null);

                    if (propertyValue is not null)
                        allPermissions.Add(propertyValue.ToString());
                }
            }

            return allPermissions;
        }

        public static async Task SeedSampleDataAsync(ApplicationDbContext context, IDateTime dateTimeService)
        {
            //Seed, if necessary
            if (!context.DocumentTypes.Any())
            {
                context.DocumentTypes.Add(new Domain.Entities.DocumentType() { Name = "Document", Description = "Document" });
                context.DocumentTypes.Add(new Domain.Entities.DocumentType() { Name = "PDF", Description = "PDF" });
                context.DocumentTypes.Add(new Domain.Entities.DocumentType() { Name = "Image", Description = "Image" });
                context.DocumentTypes.Add(new Domain.Entities.DocumentType() { Name = "Other", Description = "Other" });
                await context.SaveChangesAsync();
                context.Serilogs.Add(new Domain.Entities.Log.Serilog() { Message = "Initial add document types", Level = "Information", UserName = "System", TimeStamp = dateTimeService.Now });
                await context.SaveChangesAsync();
            }
            if (!context.KeyValues.Any())
            {
                context.KeyValues.Add(new Domain.Entities.KeyValue() { Name = "Status", Value = "initialization", Text = "initialization", Description = "Status of workflow" });
                context.KeyValues.Add(new Domain.Entities.KeyValue() { Name = "Status", Value = "processing", Text = "processing", Description = "Status of workflow" });
                context.KeyValues.Add(new Domain.Entities.KeyValue() { Name = "Status", Value = "pending", Text = "pending", Description = "Status of workflow" });
                context.KeyValues.Add(new Domain.Entities.KeyValue() { Name = "Status", Value = "finished", Text = "finished", Description = "Status of workflow" });
                context.KeyValues.Add(new Domain.Entities.KeyValue() { Name = "Region", Value = "CNC", Text = "CNC", Description = "Region of Customer" });
                context.KeyValues.Add(new Domain.Entities.KeyValue() { Name = "Region", Value = "CNN", Text = "CNN", Description = "Region of Customer" });
                context.KeyValues.Add(new Domain.Entities.KeyValue() { Name = "Region", Value = "CNS", Text = "CNS", Description = "Region of Customer" });
                context.KeyValues.Add(new Domain.Entities.KeyValue() { Name = "Region", Value = "Oversea", Text = "Oversea", Description = "Region of Customer" });
                await context.SaveChangesAsync();
                context.Serilogs.Add(new Domain.Entities.Log.Serilog() { Message = "Initial add key values", Level = "Information", UserName = "System", TimeStamp = dateTimeService.Now });
                await context.SaveChangesAsync();
            }
            if (!context.Customers.Any())
            {
                context.Customers.Add(new Domain.Entities.Customer() { Name = "SmartAdmin", AddressOfEnglish = "https://wrapbootstrap.com/theme/smartadmin-responsive-webapp-WB0573SK0", GroupName = "SmartAdmin", Address = "https://wrapbootstrap.com/theme/smartadmin-responsive-webapp-WB0573SK0", Sales = "GotBootstrap", RegionSalesDirector = "GotBootstrap", Region = "CNC", NameOfEnglish = "SmartAdmin", PartnerType = Domain.Enums.PartnerType.TP, Contact = "GotBootstrap", Email = "drlantern@gotbootstrap.com" });
                await context.SaveChangesAsync();

                context.Serilogs.Add(new Domain.Entities.Log.Serilog() { Message = "Initial add customer", Level = "Information", UserName = "System", TimeStamp = dateTimeService.Now });
                await context.SaveChangesAsync();
            }
        }
    }
}
