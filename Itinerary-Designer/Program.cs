using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Trips.Data;
using Trips.Models;
using Exchange.Services;

var builder = WebApplication.CreateBuilder(args);

// Define the connection string and server version
var connectionString = "server=localhost;user=designer;password=K9l0m15?/;database=itinerary";
var serverVersion = new MySqlServerVersion(new Version(8, 0, 37));

// Configure DbContext with MySQL
builder.Services.AddDbContext<TripDbContext>(dbContextOptions =>
    dbContextOptions.UseMySql(connectionString, serverVersion)
);

// Configure Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<TripDbContext>();

// Add other services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddTransient<ExchangeRatesApiService>();

var app = builder.Build();

// Create scope to configure roles and admin user
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Ensure Admin role exists
    var adminRoleExists = await roleManager.RoleExistsAsync("Admin");
    if (!adminRoleExists)
    {
        await roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });
    }

    // Ensure Admin user exists
    var adminUser = await userManager.FindByEmailAsync("admin@admin.com");
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin@admin.com",
            Email = "admin@admin.com"
        };

        await userManager.CreateAsync(adminUser, "adminadmin");

        // Assign Admin role to Admin user
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "users",
    pattern: "User/{action=Index}/{id?}",
    defaults: new { controller = "Users", action = "Index" });

app.Run();
