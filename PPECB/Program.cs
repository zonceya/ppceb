using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using PPECB.Data;
using PPECB.Domain.Entities;
using PPECB.Services.Interfaces;
using PPECB.Services.Services;
using PPECB.Services.Validators;
using PPECB.Services.Generators;
using PPECB.Services.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add logging to see what's happening
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity with more detailed errors
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Make password requirements simpler for testing
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ===== ADD THIS COOKIE CONFIGURATION =====
// Add this after AddDefaultTokenProviders()
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "PPECB_Auth";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Register Services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryCodeValidator, CategoryCodeValidator>();
builder.Services.AddScoped<IProductCodeGenerator, ProductCodeGenerator>();
builder.Services.AddScoped<IImageUploadHelper, ImageUploadHelper>();
builder.Services.AddScoped<IExcelService, ExcelService>();
// Add MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure pipeline
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();