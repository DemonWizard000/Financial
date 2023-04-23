using System.Security.Claims;
using System.Text.Json.Serialization;
using Financial.DAL;
using Financial.Entities;
using Financial.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add services to the container.
services.AddControllers()
    .AddJsonOptions(opts => {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opts.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        opts.JsonSerializerOptions.PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance;
    });

services.AddDbContext<FinancialContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Financial") ??
    throw new InvalidOperationException("Connection string 'Financial' not found.")));

services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<FinancialContext>()
    .AddDefaultTokenProviders()
    .AddRoles<IdentityRole>();

services.Configure<IdentityOptions>(opt =>
{
    opt.Password.RequiredLength = 6;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireDigit = true;
    opt.Password.RequireLowercase = true;
    opt.Password.RequireUppercase = false;
    opt.User.RequireUniqueEmail = true;
    opt.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
    opt.Lockout.MaxFailedAccessAttempts = 3;
    opt.Lockout.DefaultLockoutTimeSpan = System.TimeSpan.FromMinutes(10);
});

services.AddDistributedMemoryCache();
services.AddSession(options =>
{
    // options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:44462")
                             //.AllowCredentials()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
        });
});

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
app.UseAuthentication();
app.UseAuthorization();
app.UseCors(MyAllowSpecificOrigins);
app.UseSession();

app.UseMiddleware<JwtMiddleware>();
app.MapFallbackToFile("index.html");
// app.UseEndpoints(endpoints => endpoints.MapControllers());
app.MapControllerRoute(
    name: "default",
    pattern: "api/{controller}/{action=Index}/{id?}");
app.Run();
