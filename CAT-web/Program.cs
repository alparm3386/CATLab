using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CATWeb.Data;
using CATWeb.Models;
using CATWeb.Services.CAT;
using CATWeb.Services.MT;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using CATWeb.Areas.Identity.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.Cookies;
using CATWeb.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<CATWebContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CATWebContext") ?? throw new InvalidOperationException("Connection string 'CATWebContext' not found.")));

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityDbContext") ?? throw new InvalidOperationException("Connection string 'IdentityDbContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<JwtService>();

// Add Razor Pages (needed for Identity)
builder.Services.AddRazorPages();

builder.Services.AddScoped<CATClientService>();
builder.Services.TryAddEnumerable(new[]
    {
        // Type-based services
        ServiceDescriptor.Singleton<IMachineTranslator, MMT>(),
        //ServiceDescriptor.Singleton<IMachineTranslator, MachineTranslator2>(),
    });

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<IdentityDbContext>();

builder.Services.AddSession();
builder.Services.AddMemoryCache();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
    app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Use Authentication
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();


app.UseStaticFiles();
app.UseDefaultFiles();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Add endpoint routing for Razor pages
app.MapRazorPages();

app.Run();
