using AutoMapper;
using CAT.Areas.Identity.Data;
using CAT.Configuration;
using CAT.Data;
using CAT.Services;
using CAT.Infrastructure.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CAT.Services.Common;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CAT.Services.MT;
using CAT.Enums;
using Microsoft.Extensions.FileProviders;
using CAT.Areas.BackOffice.Services;
using Microsoft.Extensions.DependencyInjection;
using CAT.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var identityConnectionString = builder.Configuration.GetConnectionString("IdentityDbConnection") ?? throw new InvalidOperationException("Connection string 'IdentityDbConnection' not found.");
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(identityConnectionString));
var mainConnectionString = builder.Configuration.GetConnectionString("MainDbConnection") ?? throw new InvalidOperationException("Connection string 'MainDbConnection' not found.");
builder.Services.AddDbContext<MainDbContext>(options =>
    options.UseSqlServer(mainConnectionString));
var translationUnitsConnectionString = builder.Configuration.GetConnectionString("TranslationUnitsDbConnection") ?? throw new InvalidOperationException("Connection string 'TranslationUnitsDbConnection' not found.");
builder.Services.AddDbContext<TranslationUnitsDbContext>(options =>
    options.UseSqlServer(translationUnitsConnectionString));

builder.Services.AddTransient<DbContextContainer>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<JobService>();
builder.Services.AddScoped<CATConnector>();
builder.Services.AddScoped<IDocumentProcessor, DocumentProcessor>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IMonitoringService, MonitoringService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddAntiforgery(options => options.HeaderName = "XSRF-TOKEN");

builder.Services.TryAddEnumerable(new[]
    {
        // Type-based services
        ServiceDescriptor.Singleton<IMachineTranslator, MMT>(),
        //ServiceDescriptor.Singleton<IMachineTranslator, MachineTranslator2>(),
    });

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>();

builder.Services.AddRazorPages();

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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admins", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Linguists", policy => policy.RequireRole("Linguist"));
    options.AddPolicy("Clients", policy => policy.RequireRole("Client"));
    // Add other policies as needed
});


//the logger
builder.Logging.AddProvider(new Log4NetLoggerProvider("log4net.config"));

builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

var app = builder.Build();

//EnsureRoleCreated(app).Wait();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.MapAreaControllerRoute(
    name: "BackOfficeRoute",
    areaName: "BackOffice",
    pattern: "BackOffice/{controller=Home}/{action=Index}/{id?}")
.RequireAuthorization("Admins");  // Apply the policy
app.MapAreaControllerRoute(
    name: "ClientsRoute",
    areaName: "ClientsPortal",
    pattern: "ClientsPortal/{controller=Home}/{action=Index}/{id?}")
//.RequireAuthorization("Admins")
.RequireAuthorization("Clients"); ;  // Apply the policy
app.MapAreaControllerRoute(
    name: "LinguistsRoute",
    areaName: "LinguistsPortal",
    pattern: "LinguistsPortal/{controller=Home}/{action=Index}/{id?}")
.RequireAuthorization("Linguists");  // Apply the policy

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "WebUI")),
    RequestPath = ""  // serve from root
});

// Serve from the default wwwroot directory
app.UseStaticFiles();

// Use CORS middleware here
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseRouting();

//areas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

//default
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseAuthentication();
app.UseMiddleware<RoleBasedRedirectMiddleware>();
app.UseAuthorization();

app.MapRazorPages();

app.Use(async (context, next) =>
{
    if (context.Request.Path.Value == "/")
    {
        context.Response.Redirect("/BackOffice/Monitoring");
        return;
    }
    await next.Invoke();
});

//Middlewares
app.UseMiddleware<AuthDebugMiddleware>();
//app.UseMiddleware<TransactionMiddleware>();

app.Run();

async System.Threading.Tasks.Task EnsureRoleCreated(WebApplication app)
{
    using var serviceScope = app.Services.CreateScope();
    var serviceProvider = serviceScope.ServiceProvider;
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    if (!await roleManager.RoleExistsAsync("Client"))
    {
        await roleManager.CreateAsync(new IdentityRole("Client"));
    }
    if (!await roleManager.RoleExistsAsync("Linguist"))
    {
        await roleManager.CreateAsync(new IdentityRole("Linguist"));
    }
}
