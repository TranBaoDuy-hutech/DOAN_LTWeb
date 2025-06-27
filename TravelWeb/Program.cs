using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using TravelWeb.Data;
using TravelWeb.Services;
using OfficeOpenXml;
using Microsoft.AspNetCore.Identity.UI.Services;
using TravelWeb.Models;
using Google.Api;
using DinkToPdf.Contracts;
using DinkToPdf;
using TravelWeb.Models.OpenWeather;

var builder = WebApplication.CreateBuilder(args);
// Add HttpClient
builder.Services.AddHttpClient("MoMo", client =>
{
    client.BaseAddress = new Uri("https://test-payment.momo.vn/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
// Set EPPlus license
ExcelPackage.License.SetNonCommercialPersonal("Student");
builder.Services.Configure<WeatherForecastResponse>(
    builder.Configuration.GetSection("WeatherSettings"));
builder.Services.AddHttpClient();


// Configure authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
.AddFacebook(options =>
{
    options.AppId = "704853985596831";
    options.AppSecret = "dc92ea9b4c5a05c4185d04b4e218be32";
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
})
.AddGitHub(options =>
{
    options.ClientId = builder.Configuration["Authentication:GitHub:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];
});


// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddDefaultTokenProviders()
    .AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add services
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure middleware pipeline
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
app.UseSession();

app.MapRazorPages();
app.MapControllerRoute(
    name: "Admin",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tour}/{action=Index}/{id?}");
app.MapControllers();

app.Run();