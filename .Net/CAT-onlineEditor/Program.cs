using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CAT.Data;
using CAT.Models;
using CAT.Services.Common;
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
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration.Json;

var builder = WebApplication.CreateBuilder(args);

//AddSQLServerContext(builder)
AddMySqlContext(builder);
AddConfiguration(builder);

builder.Services.AddTransient<DbContextContainer>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<JobService>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<CatConnector>();
builder.Services.AddSingleton<ICatClientFactory, CatClientFactory>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddHttpClient();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>();
// Add Razor Pages (needed for Identity)
builder.Services.AddRazorPages();

builder.Services.AddSession();
builder.Services.AddMemoryCache();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder!.Configuration["Jwt:Key"]!))
    };
});

//the logger
builder.Logging.AddProvider(new Log4NetLoggerProvider("log4net.config"));

var app = builder.Build();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

using (var scope = app.Services.CreateScope())
{
    //var services = scope.ServiceProvider

    //SeedData.Initialize(services)
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

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "online-editor-ui/build")),
    RequestPath = "/onlineEditor"  // serve from root
});

app.UseStaticFiles();

// Use CORS middleware here
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

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
    logger.LogInformation("Incoming request: {RequestMethod} {RequestPath}", context.Request.Method, context.Request.Path);

    // Continue processing
    await next.Invoke();

    // After the response
    logger.LogInformation("Response: {StatusCode}", context.Response.StatusCode);
});

app.Run();

void AddConfiguration(WebApplicationBuilder builder)
{
    // Register the database configuration provider
    builder.Services.AddSingleton<DatabaseConfigurationSource>(provider =>
    {
        return new DatabaseConfigurationSource(provider);
    });

    builder.Services.AddSingleton<IConfiguration>(provider =>
    {
        var dbConfigSource = provider.GetRequiredService<DatabaseConfigurationSource>();
        return new ConfigurationRoot(new List<Microsoft.Extensions.Configuration.IConfigurationProvider>
        {
            new JsonConfigurationProvider(new JsonConfigurationSource
            {
                Optional = true, // Make the appsettings.json file optional
                FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory()),
                Path = "appsettings.json"
            }),
            dbConfigSource.GetConfigurationProvider() // Include the database configuration source
        });
    });
}

void AddMySqlContext(WebApplicationBuilder builder)
{
    var identityConnectionString = builder.Configuration.GetConnectionString("IdentityDbConnection");
    builder.Services.AddDbContext<IdentityDbContext>(options =>
    {
        options.UseMySql(identityConnectionString, new MySqlServerVersion(new Version(8, 0, 34)), mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure();
        });
    });
    var mainConnectionString = builder.Configuration.GetConnectionString("MainDbConnection");
    builder.Services.AddDbContext<MainDbContext>(options =>
    {
        options.UseMySql(mainConnectionString, new MySqlServerVersion(new Version(8, 0, 34)), mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure();
        });
    });

    var translationUnitsConnectionString = builder.Configuration.GetConnectionString("TranslationUnitsDbConnection");
    builder.Services.AddDbContext<TranslationUnitsDbContext>(options =>
    {
        options.UseMySql(translationUnitsConnectionString, new MySqlServerVersion(new Version(8, 0, 34)), mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure();
        });
    });
}

#pragma warning disable CS8321 // Disable warning CS8321
void AddSQLServerContext(WebApplicationBuilder builder)
{
    //string dbPassword = Environment.GetEnvironmentVariable("IDENTITY_DB_PASSWORD")
    var dbPassword = "AVNS_F-W_2EL1ueOt82f8wQY";
    var identityConnectionString = builder.Configuration.GetConnectionString("IdentityDbConnection") + $"Password={dbPassword};";
    builder.Services.AddDbContext<IdentityDbContext>(options => options.UseSqlServer(identityConnectionString));
    var mainConnectionString = builder.Configuration.GetConnectionString("MainDbConnection") + $"Password={dbPassword};";
    builder.Services.AddDbContext<MainDbContext>(options => options.UseSqlServer(mainConnectionString));
    var translationUnitsConnectionString = builder.Configuration.GetConnectionString("TranslationUnitsDbConnection") + $"Password={dbPassword};";
    builder.Services.AddDbContext<TranslationUnitsDbContext>(options => options.UseSqlServer(translationUnitsConnectionString));
}
#pragma warning restore CS8321 // Re-enable warning CS8321