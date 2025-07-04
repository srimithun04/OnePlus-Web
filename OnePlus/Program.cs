using Microsoft.EntityFrameworkCore;
using OnePlus.Data;
using OnePlus.Services; // Add this using statement for your services

var builder = WebApplication.CreateBuilder(args);

// --- Add services to the container ---

// 1. Register your custom services for dependency injection
builder.Services.AddScoped<IUamService, UamService>();
builder.Services.AddHttpContextAccessor(); // Often needed for session access in services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IHomeService, HomeService>();
// 2. Configure the DbContext with the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();


var app = builder.Build();

// --- Configure the HTTP request pipeline ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// IMPORTANT: Session must be used before routing
app.UseSession();

app.UseRouting();

app.UseAuthorization();

// This route correctly sets the UamController's Login action as the default page
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// This makes your Program class visible to the EF Core tools for migrations
public partial class Program { }
