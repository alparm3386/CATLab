using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CAT.Data;
using CAT.Models;
using CAT.Services.CAT;
using CAT.Services.MT;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.Cookies;
using CAT.Services;
using CAT.Configuration;
using AutoMapper;
using log4net;
using System.Reflection;
using CAT.Infrastructure.Logging;
using CAT.Areas.Identity.Data;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityDbConnection") ?? throw new InvalidOperationException("Connection string 'IdentityDbConnection' not found.")));
builder.Services.AddDbContext<MainDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MainDbConnection") ?? throw new InvalidOperationException("Connection string 'MainDbConnection' not found.")));
builder.Services.AddDbContext<TranslationUnitsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TranslationUnitsDbConnection") ?? throw new InvalidOperationException("Connection string 'TranslationUnitsDbConnection' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<JobService>();
builder.Services.AddScoped<CATConnector>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddHttpClient();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<IdentityDbContext>();
// Add Razor Pages (needed for Identity)
builder.Services.AddRazorPages();

builder.Services.TryAddEnumerable(new[]
    {
        // Type-based services
        ServiceDescriptor.Singleton<IMachineTranslator, MMT>(),
        //ServiceDescriptor.Singleton<IMachineTranslator, MachineTranslator2>(),
    });

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

//the logger
builder.Logging.AddProvider(new Log4NetLoggerProvider("log4net.config"));

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

using (var scope = app.Services.CreateScope())
{
    //var services = scope.ServiceProvider;

    //SeedData.Initialize(services);
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

app.Use(async (context, next) =>
{
    logger.LogInformation($"Incoming request: {context.Request.Method} {context.Request.Path}");

    // Continue processing
    await next.Invoke();

    // After the response
    logger.LogInformation($"Response: {context.Response.StatusCode}");
});

app.Run();
