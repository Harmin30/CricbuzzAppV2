using CricbuzzAppV2.Data;
using CricbuzzAppV2.Filters;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new SessionCheckAttribute()); // global session check
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Default public route → UserPortal
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=UserPortal}/{action=Index}/{id?}");

// Friendly admin route → /admin redirects to Account/Login
app.MapControllerRoute(
    name: "admin",
    pattern: "admin",
    defaults: new { controller = "Account", action = "Login" });

app.Run();
