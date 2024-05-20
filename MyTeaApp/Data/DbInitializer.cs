using MyTeaApp.Data;

using Microsoft.AspNetCore.Identity;
using MyTeaApp.Models;

/// <summary>
/// Initializes database with everything needed for the system to work properly.
/// </summary>
///
public class DbInitializer
{
    private static Array roles = new[] { "Admin", "Employee", "Manager" };
    private static Array initWBS = new[] { "Ferias", "Day-off", "Sem tarefa", "Implementação e Desenvolvimento" };

    private static IServiceProvider _sp;

    /// <summary>
    /// Ensures roles ("Admin", "Employee", "Manager") are created previously.
    /// </summary>
    /// 
    /// <returns>Void</returns>
    /// 
    /// <exception cref="ArgumentNullException"></exception>
    private static async Task CreateRoles()
    {
        RoleManager<IdentityRole> roleManager = _sp.GetRequiredService<RoleManager<IdentityRole>>();

        if(roleManager == null)
        {
            throw new ArgumentNullException(nameof(roleManager));
        }

        foreach(string role in roles)
        {
            if(!await roleManager.RoleExistsAsync(role)) {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task CreateDefaultDepartment()
    {
        var context = _sp.GetRequiredService<ApplicationDbContext>();
        bool depExists = context.Department.Any(d => d.DepartmentName == "HR");
        if(!depExists)
        {
            Department dep = new Department()
            {
                DepartmentName = "HR"
            };
            context.Department.Add(dep);
            await context.SaveChangesAsync();
        }
    }

    
    /// <summary>
    /// Initializes database.
    /// </summary>
    /// 
    /// <param name="serviceProvider">IServiceProvider</param>
    /// <returns>Void</returns>
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        _sp = serviceProvider;

        var context = _sp
            .GetRequiredService<ApplicationDbContext>();

        
        context.Database.EnsureCreated();

        await CreateRoles();

        await CreateDefaultDepartment();

        if (context.WBS.Any())
        {
            return;
        }
        else
        {
            context.WBS.AddRange(
                new WBS
                {
                    WbsName = "Vacation",
                    CodWbs = "WBS0085749",
                    Description = "Vacation - employee",
                    IsChargeable = true,
                },
                new WBS
                {
                    WbsName = "Day-Off",
                    CodWbs = "WBS8574950",
                    Description = "Day-Off - employee",
                    IsChargeable = true,
                },
                new WBS
                {
                    WbsName = "No task",
                    CodWbs = "WBS4700086",
                    Description = "No task - employee",
                    IsChargeable = true,
                },
                new WBS
                {
                    WbsName = "Implementation",
                    CodWbs = "WBS9665557",
                    Description = "Implementation - employee",
                    IsChargeable = true,
                },
                new WBS
                {
                    WbsName = "Development",
                    CodWbs = "WBS2574100",
                    Description = "Development - employee",
                    IsChargeable = true,
                }


            );

            context.SaveChanges();
        }

        //var roleManager = serviceProvider
        //    .GetRequiredService<RoleManager<IdentityRole>>();

        //var userManager = serviceProvider
        //    .GetRequiredService<UserManager<IdentityUser>>();

        //var config = serviceProvider
        //    .GetRequiredService<IConfiguration>();

        //var roleName = "Administrator";

        //if (!await roleManager.RoleExistsAsync(roleName))
        //{
        //    await roleManager.CreateAsync(new IdentityRole(roleName));
        //}

        //var adminEmail = config["AdminCredentials:Email"];
        //var adminPassword = config["AdminCredentials:Password"];

        //var admin = await userManager.FindByEmailAsync(adminEmail);

        //if (admin == null)
        //{
        //    admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        //    var result = await userManager.CreateAsync(admin, adminPassword);

        //    if (result.Succeeded)
        //    {
        //        await userManager.AddToRoleAsync(admin, roleName);
        //    }
        //}
        //else
        //{
        //    if (!await userManager.IsInRoleAsync(admin, roleName))
        //    {
        //        await userManager.AddToRoleAsync(admin, roleName);
        //    }
        //}
    }
}