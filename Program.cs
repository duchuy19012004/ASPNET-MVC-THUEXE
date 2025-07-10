using Microsoft.EntityFrameworkCore;
using bike.Repository;
using bike.Attributes;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Thêm Session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Thêm HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Đăng ký Services
builder.Services.AddScoped<bike.Services.ICartService, bike.Services.CartService>();
builder.Services.AddScoped<bike.Services.IDamageCompensationService, bike.Services.DamageCompensationService>();

// Cấu hình Entity Framework với SQL Server
builder.Services.AddDbContext<BikeDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BikeConnection")));

// THÊM AUTHENTICATION 
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

// Thêm Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

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

// Thêm UseSession() trước UseAuthentication()
app.UseSession();

// Thêm UseAuthentication() trước UseAuthorization()
app.UseAuthentication(); 

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();