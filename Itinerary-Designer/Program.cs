using System;
using Exchange.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using Microsoft.EntityFrameworkCore;
using Trips.Data;

// using Exchange.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "server=localhost;user=designer;password=K9l0m15?/;database=itinerary";

var serverVersion = new MySqlServerVersion(new Version(8, 0, 37));

builder.Services.AddDbContext<TripDbContext>(dbContextOptions =>
    dbContextOptions.UseMySql(connectionString, serverVersion)
);

//--- end of connection syntax

builder
    .Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<TripDbContext>();

builder.Services.AddTransient<ExchangeRatesApiService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

   
    var adminRoleExists = await roleManager.RoleExistsAsync("Admin");
    if (!adminRoleExists)
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

 
    var adminUser = await userManager.FindByEmailAsync("notkyle@notkyle.com");
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = "admin@admin.com",
            Email = "admin@admin.com"
        };

        await userManager.CreateAsync(adminUser, "adminadmin");

        
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    endpoints.MapControllerRoute(
        name: "users",
        pattern: "User/{action=Index}/{id?}",
        defaults: new { controller = "Users", action = "Index" });
});

app.Run();
